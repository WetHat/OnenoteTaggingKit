// Author: WetHat | (C) Copyright 2013 - 2017 WetHat Lab, all rights reserved
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace WetHatLab.OneNote.TaggingKit.common
{
    /// <summary>
    /// Local representation of a OneNote Page
    /// </summary>
    /// <remarks>Supports tag related operations.</remarks>
    internal class OneNotePageProxy
    {
        internal static readonly String META_NAME = "TaggingKit.PageTags";

        private const int META_IDX = 3;

        private const int QUICKSTYLEDEF_IDX = 1;

        private const int TAGDEF_IDX = 0;

        private const int TITLE_IDX = 7;

        // Sequence of elements below the page tag
        //<xsd:element name="TagDef" type="TagDef" minOccurs="0" maxOccurs="unbounded"/>
        //<xsd:element name="QuickStyleDef" type="QuickStyleDef" minOccurs="0" maxOccurs="unbounded"/>
        //<xsd:element name="XPSFile" type="XPSFile" minOccurs="0" maxOccurs="unbounded"/>
        //<xsd:element name="Meta" type="Meta" minOccurs="0" maxOccurs="unbounded"/>
        //<xsd:element name="MediaPlaylist" type="MediaPlaylist" minOccurs="0"/>
        //<xsd:element name="MeetingInfo" type="MeetingInfo" minOccurs="0"/>
        //<xsd:element name="PageSettings" type="PageSettings" minOccurs="0"/>
        //<xsd:element name="Title" type="Title" minOccurs="0"/>
        private static readonly String[] ELEMENT_SEQUENCE = { "TagDef", "QuickStyleDef", "XPSFile", "Meta", "MediaPlaylist", "MeetingInfo", "PageSettings", "Title" };

        private DateTime _lastModified;
        private XElement _meta;
        private XNamespace _one;

        // the OneNote application object
        private OneNoteProxy _onenote;

        private string[] _originalTags;
        private XElement _page;

        private XDocument _pageDoc; // the OneNote page document
        private XElement _pageTagsOE; // <one:T> element with tags

        private string[] _tags;
        private XElement _titleOE;

        internal OneNotePageProxy(OneNoteProxy onenoteApp, string pageID)
        {
            _onenote = onenoteApp;
            PageID = pageID;
            LoadOneNotePage();
            if (_pageTagsOE != null)
            {
                if (_meta != null)
                {
                    _originalTags = ParseTags(_meta.Attribute("content").Value);
                }
                else
                {
                    _originalTags = ParseTags(_pageTagsOE.Value);
                }
            }
            if (_originalTags == null)
            {
                _originalTags = new string[0];
            }
        }

        internal bool IsDeleted
        {
            get
            {
                XAttribute recycleBinAtt = _page.Attribute("isInRecycleBin");

                return recycleBinAtt != null ? bool.Parse(recycleBinAtt.Value) : false;
            }
        }

        /// <summary>
        /// Get or set the unique ID of the OneNote Page
        /// </summary>
        internal string PageID { get; private set; }

        internal string[] PageTags
        {
            get
            {
                return _tags ?? _originalTags;
            }

            set
            {
                _tags = value;
            }
        }

        internal string Title
        {
            get
            {
                return _titleOE != null ? Regex.Replace(_titleOE.Value, "<[^<>]*>", "") : "Untitled page";
            }
        }

        internal static string[] ParseTags(string tags)
        {
            if (!string.IsNullOrEmpty(tags))
            {
                // remove all HTML markup
                tags = Regex.Replace(tags, "<[^<>]+>", String.Empty);
                string[] parsedTags = tags.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                // normalize tags
                for (int i = 0; i < parsedTags.Length; i++)
                {
                    parsedTags[i] = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(parsedTags[i].Trim());
                }
                return parsedTags;
            }
            return new string[0];
        }

        /// <summary>
        /// Save all changes to the page to OneNote
        /// </summary>
        internal void Update()
        {
            if (_tags != null)
            {
                string[] savedTags = _tags;
                ApplyTagsToPage();
                try
                {
                    _onenote.UpdatePage(_pageDoc, _lastModified);
                }
                catch (COMException ce)
                {
                    unchecked
                    {
                        if (ce.ErrorCode == (int)0x80042010)
                        { // try again after concurrent page modification
                            LoadOneNotePage();
                            PageTags = savedTags;
                            ApplyTagsToPage();
                            _onenote.UpdatePage(_pageDoc, _lastModified);
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Add a new element at the correct position to a page
        /// </summary>
        /// <param name="element"> element to add</param>
        /// <param name="sequence">index of element name in the schema sequence</param>
        private void addElementToPage(XElement element, int sequence)
        {
            // locate a successor
            for (int i = sequence; i < ELEMENT_SEQUENCE.Length; i++)
            {
                XElement first = _page.Elements(_one.GetName(ELEMENT_SEQUENCE[i])).FirstOrDefault();
                if (first != null)
                {
                    first.AddBeforeSelf(element);
                    return;
                }
            }
            _page.Add(element);
        }

        private void ApplyTagsToPage()
        {
            if (_tags == null)
            {
                return;
            }
            XName tagName = _one.GetName("Tag");

            // collect all tag definitions
            XName tagdefName = _one.GetName("TagDef");
            IEnumerable<XElement> tagDefs = _page.Elements(tagdefName);

            int nextTagDefIndex = tagDefs.Count(); // next index for creating new tags

            if (_pageTagsOE == null && _tags.Length > 0)
            {
                // Create a style for the tags - if needed <one:QuickStyleDef index="1"
                // name="cite" fontColor="#595959" highlightColor="automatic"
                // font="Calibri" fontSize="9"
                XName styledefName = _one.GetName("QuickStyleDef");

                IEnumerable<XElement> quickstyleDefs = _page.Elements(styledefName);

                XElement tagStyle = quickstyleDefs.FirstOrDefault(d => d.Attribute("name").Value == Properties.Settings.Default.TagOutlineStyle_Name);

                if (tagStyle == null)
                {
                    tagStyle = new XElement(styledefName,
                                            new XAttribute("index", (quickstyleDefs.Count() + 1).ToString()),
                                            new XAttribute("name", Properties.Settings.Default.TagOutlineStyle_Name),
                                            new XAttribute("fontColor", "#595959"),
                                            new XAttribute("highlightColor", "automatic"),
                                            new XAttribute("bold", Properties.Settings.Default.TagOutlineStyle_Font.Bold.ToString().ToLower()),
                                            new XAttribute("italic", Properties.Settings.Default.TagOutlineStyle_Font.Italic.ToString().ToLower()),
                                            new XAttribute("font", Properties.Settings.Default.TagOutlineStyle_Font.Name),
                                            new XAttribute("fontSize", Properties.Settings.Default.TagOutlineStyle_Font.Size));
                    addElementToPage(tagStyle, QUICKSTYLEDEF_IDX);
                }

                // create tag definition for the tags outline like so: <one:TagDef
                // index="3" name="Page Tags" type="23" symbol="26" />
                XElement pageTagsDef = tagDefs.FirstOrDefault(t => t.Attribute("name").Value == "Page Tags");
                if (pageTagsDef == null)
                {
                    pageTagsDef = new XElement(tagdefName,
                                               new XAttribute("index", nextTagDefIndex++),
                                               new XAttribute("name", "Page Tags"),
                                               new XAttribute("type", "23"),
                                               new XAttribute("symbol", "26"));
                    _page.AddFirst(pageTagsDef);
                }

                // Create an outline for the page tags
                //
                //<one:Outline author="Peter Ernst" authorInitials="PE" lastModifiedBy="Peter Ernst" lastModifiedByInitials="PE" objectID="{E470786C-A904-4E9F-AC3B-0D9F36B6FC54}{14}{B0}" lastModifiedTime="2013-12-06T16:04:48.000Z">
                //  <one:Position x="236.249984741211" y="42.1500015258789" z="0" />
                //  <one:Size width="90.8847274780273" height="10.9862976074219" />
                //  <one:OEChildren>
                //    <one:OE objectID="{E470786C-A904-4E9F-AC3B-0D9F36B6FC54}{15}{B0}" lastModifiedTime="2013-12-06T16:04:48.000Z" quickStyleIndex="1" creationTime="2013-12-06T16:03:50.000Z">
                //      <one:Tag index="0" completed="true" creationDate="2013-12-06T15:55:59.000Z" completionDate="2013-12-06T15:55:59.000Z" />
                //      <one:T><![CDATA[Gdfgdf, sdfdsf]]></one:T>
                //    </one:OE>
                //  </one:OEChildren>
                //</one:Outline>
                XElement outline = new XElement(_one.GetName("Outline"),
                                               new XElement(_one.GetName("Position"),
                                                            new XAttribute("x", "236"),
                                                            new XAttribute("y", "43"),
                                                            new XAttribute("z", "1")),
                                               new XElement(_one.GetName("Size"),
                                                            new XAttribute("width", "400"),
                                                            new XAttribute("height", "10"),
                                                            new XAttribute("isSetByUser", "true")),
                                               new XElement(_one.GetName("OEChildren"),
                                             (_pageTagsOE = new XElement(_one.GetName("OE"),
                                                                         new XAttribute("quickStyleIndex", tagStyle.Attribute("index").Value),
                                                                         new XElement(tagName,
                                                                                      new XAttribute("index", pageTagsDef.Attribute("index").Value),
                                                                                      new XAttribute("completed", "true"))))));
                _page.Add(outline);
            }
            else if (_pageTagsOE != null && _tags.Length == 0)
            { // remove the empty page tag outline
                XElement outline = _pageTagsOE.Parent.Parent;
                _onenote.DeletePageContent(PageID, outline.Attribute("objectID").Value);
                outline.Remove();
                _pageTagsOE = null;
            }
            // add new tags to the page title and remove obsolete ones
            // <one:Title lang="de">
            //  <one:OE objectID="{9A0ACA13-6D63-4137-8821-5D7C5408BB6C}{15}{B0}" lastModifiedTime="2013-12-08T14:08:11.000Z" quickStyleIndex="0" author="Peter Ernst" authorInitials="PE" lastModifiedBy="Peter Ernst" lastModifiedByInitials="PE" creationTime="2013-12-08T14:08:11.000Z">
            //    <one:Tag index="0" completed="true" creationDate="2013-12-06T20:31:43.000Z" completionDate="2013-12-06T20:31:43.000Z" />
            //    <one:Tag index="1" completed="true" creationDate="2013-12-06T20:34:05.000Z" completionDate="2013-12-06T20:34:05.000Z" />
            //    <one:Tag index="2" completed="true" creationDate="2013-12-06T20:35:41.000Z" completionDate="2013-12-06T20:35:41.000Z" />
            //    <one:T><![CDATA[Test Addin ]]></one:T>
            //  </one:OE>
            //</one:Title>

            // Locate tag definitions for existing page tags and record their indices
            // <one:TagDef index="0" name="Test Tag 1" type="0" symbol="0" /> <one:TagDef
            // index="1" name="Test Tag 2" type="1" symbol="0" />
            IDictionary<string, string> tagToIndexMap = new Dictionary<string, string>();
            foreach (XElement tagdef in tagDefs.Where(d => d.Attribute("symbol").Value == "0"
                                                      && (_originalTags.Contains(d.Attribute("name").Value)
                                                          || "#808083".Equals(d.Attribute("fontColor").Value))))
            {
                tagToIndexMap[tagdef.Attribute("name").Value] = tagdef.Attribute("index").Value;
                // make sure type is equal to index so that type is unique
                tagdef.Attribute("type").Value = tagdef.Attribute("index").Value;
                // remove any color attribute so that the title can show its original color
                if (tagdef.Attribute("fontColor") != null)
                {
                    tagdef.Attribute("fontColor").Remove();
                }
            }

            // add new tag definitions, if needed
            foreach (string tag in _tags)
            {
                string strIndex;
                if (!tagToIndexMap.TryGetValue(tag, out strIndex))
                { // create a new definition for this tag
                    strIndex = (nextTagDefIndex++).ToString();
                    XElement tagdef = new XElement(tagdefName,
                                                   new XAttribute("index", strIndex),
                                                   new XAttribute("name", tag),
                                                   new XAttribute("type", strIndex),
                                                   new XAttribute("symbol", "0"));
                    _page.AddFirst(tagdef);
                }

                // tag the title with an invisible tag
                XElement titleTag = _titleOE.Elements(tagName).FirstOrDefault(t => t.Attribute("index").Value == strIndex);

                if (titleTag == null)
                {
                    _titleOE.AddFirst(new XElement(_one.GetName("Tag"),
                                                  new XAttribute("index", strIndex),
                                                  new XAttribute("completed", "true")));
                }

                tagToIndexMap.Remove(tag);
            }

            // remove unused tags from the title
            foreach (string strIndex in tagToIndexMap.Values)
            {
                XElement tag = _titleOE.Elements(tagName).FirstOrDefault(t => t.Attribute("index").Value == strIndex);
                if (tag != null)
                {
                    tag.Remove();
                }
            }

            string strTags = string.Join(", ", _tags);

            // create the <one:Meta> element for page tags, if needed
            if (_meta == null && _tags.Length > 0)
            {
                _meta = new XElement(_one.GetName("Meta"),
                                    new XAttribute("name", META_NAME));
                addElementToPage(_meta, META_IDX);
            }

            if (_meta != null)
            {
                _meta.SetAttributeValue("content", strTags);
            }

            if (_pageTagsOE != null && !strTags.Equals(_pageTagsOE.Value))
            {
                // remove all old <one:T> text elements, but leave the marker tag in place.
                XName tName = _one.GetName("T");
                foreach (XElement t in _pageTagsOE.Elements(tName).ToArray())
                {
                    t.Remove();
                }
                _pageTagsOE.Add(new XElement(tName, strTags));
                // turn off spell checking
                _pageTagsOE.SetAttributeValue("lang", "yo");
            }
            _tags = null;
        }

        private void LoadOneNotePage()
        {
            _pageTagsOE = null;
            _pageDoc = _onenote.GetPage(PageID);
            _one = _pageDoc.Root.GetNamespaceOfPrefix("one");
            _page = _pageDoc.Root;

            XName metaName = _one.GetName("Meta");
            _meta = _page.Elements(metaName).FirstOrDefault(m => m.Attribute("name").Value == META_NAME);

            _lastModified = DateTime.Parse(_page.Attribute("lastModifiedTime").Value);

            XName outlineName = _one.GetName("Outline");

            // find the definition <one:TagDef> element marking the tag outline <one:TagDef
            // index="0" name="Tags" type="23" symbol="26" />
            XElement tagDef = (from d in _page.Elements(_one.GetName("TagDef"))
                               where d.Attribute("name").Value == "Page Tags" && d.Attribute("type").Value == "23" && d.Attribute("symbol").Value == "26"
                               select d).FirstOrDefault();

            // For performance reasons we are going to delete all outlines not related to tags
            // Note: Page updates will actually leave those removed outlines on the page.
            List<XElement> outlinesToDelete = new List<XElement>();

            // find <one:Outline> containing page tags.
            if (tagDef != null)
            {
                string defindex = tagDef.Attribute("index").Value;
                // locate the tag outline element looking like this
                //<one:Outline author="Peter Ernst" authorInitials="PE" lastModifiedBy="Peter Ernst" lastModifiedByInitials="PE" objectID="{E470786C-A904-4E9F-AC3B-0D9F36B6FC54}{14}{B0}" lastModifiedTime="2013-12-06T16:04:48.000Z">
                //  <one:Position x="236.249984741211" y="42.1500015258789" z="0" />
                //  <one:Size width="90.8847274780273" height="10.9862976074219" />
                //  <one:OEChildren>
                //    <one:OE objectID="{E470786C-A904-4E9F-AC3B-0D9F36B6FC54}{15}{B0}" lastModifiedTime="2013-12-06T16:04:48.000Z" quickStyleIndex="1" creationTime="2013-12-06T16:03:50.000Z">
                //      <one:Tag index="0" completed="true" creationDate="2013-12-06T15:55:59.000Z" completionDate="2013-12-06T15:55:59.000Z" />
                //      <one:T><![CDATA[Gdfgdf, sdfdsf]]></one:T>
                //    </one:OE>
                //  </one:OEChildren>
                //</one:Outline>
                //
                // For performance reasons we avoid Xpath!
                foreach (var outline in _page.Elements(outlineName))
                {
                    // the outline we are looking for has one <one:OEChildren> element with
                    // one <one:OE> element containing a <one:Tag> with the given index
                    if (_pageTagsOE == null)
                    {
                        XElement OEChildren = outline.Elements(_one.GetName("OEChildren")).FirstOrDefault();
                        if (OEChildren != null)
                        {
                            XElement OE = OEChildren.Elements(_one.GetName("OE")).FirstOrDefault();
                            if (OE != null)
                            {
                                XElement tag = (from t in OE.Elements(_one.GetName("Tag"))
                                                where t.Attribute("index").Value == defindex
                                                select t).FirstOrDefault();
                                if (tag != null)
                                { // found the outline with page tags, get the text element containing the tags
                                    _pageTagsOE = OE;
                                    // delete the Indents as they tend to cause errors
                                    XElement indents = outline.Element(_one.GetName("Indents"));
                                    if (indents != null)
                                    {
                                        indents.Remove();
                                    }
                                    continue;
                                }
                            }
                        }
                    }
                    outlinesToDelete.Add(outline); // enroll for deletion
                }
            }
            else
            {
                // record all outline elements for deletion
                foreach (var outline in _page.Elements(outlineName))
                {
                    outlinesToDelete.Add(outline);
                }
            }

            // delete outlines to make updating the page as fast as possible, knowing that
            // the update method will keep these outlines actually on the page.
            foreach (XElement outline in outlinesToDelete)
            {
                outline.Remove();
            }

            XName titleName = _one.GetName("Title");
            XElement title = _page.Elements(titleName).FirstOrDefault();
            if (title == null)
            {
                title = new XElement(titleName, new XElement(_one.GetName("OE")));
                addElementToPage(title, TITLE_IDX);
            }

            _titleOE = title.Elements(_one.GetName("OE")).FirstOrDefault();
        }
    }
}