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
        /// <summary>
        /// Enumeration of way to display tags on a Onenote Page
        /// </summary>
        private enum TagDisplay
        {
            /// <summary>
            /// Display tags as comma separated text below the title.
            /// </summary>
            BelowTitle,

            /// <summary>
            /// Display the tags a single OneNote tag whose name is comma separated tag names.
            /// </summary>
            InTitle
        }

        internal static readonly String META_NAME = "TaggingKit.PageTags";

        private const int META_IDX = 3;

        private const int QUICKSTYLEDEF_IDX = 1;

        private const int TAGDEF_IDX = 0;

        private const int TITLE_IDX = 7;

        private readonly string MarkerTagname = "Page Tags";

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

        private XElement _page;

        private XDocument _pageDoc; // the OneNote page document

        /// <summary>
        /// &lt;one:OE&gt; element holding marker tag. This is either a title OE or a OE in
        /// an outline of the page.
        /// </summary>
        private XElement _pageTagsOE;

        private readonly string[] _originalTags;

        /// <summary>
        /// The array of page tags.
        /// </summary>
        private string[] _tags;

        private XElement _titleOE;

        private XElement _markerTagDef;

        internal OneNotePageProxy(OneNoteProxy onenoteApp, string pageID) {
            _onenote = onenoteApp;
            PageID = pageID;
            LoadOneNotePage();
            var tagset = new HashSet<string>();
            // collect tags from various sources on the page
            if (_meta != null) {  // the meta tags
                foreach (string t in ParseTags(_meta.Attribute("content").Value)) {
                    tagset.Add(t);
                }
            }

            if (_markerTagDef != null && "99".Equals(_markerTagDef.Attribute("type").Value)) { // use the in-title tag
                foreach (string t in ParseTags(_markerTagDef.Attribute("name").Value)) {
                    tagset.Add(t);
                }
            }

            // get all tag definitions which could be page tags from the page header.
            var indexmap = new Dictionary<string, XElement>();
            foreach (XElement tagDef in from td in _page.Elements(_one.GetName("TagDef"))
                                        where "0".Equals(td.Attribute("symbol").Value)
                                        select td) {
                indexmap[tagDef.Attribute("index").Value] = tagDef;
            }
            // get all the hidden title tags
            foreach (XElement t in _titleOE.Elements(_one.GetName("Tag"))) {
                XElement tagdef;
                if (indexmap.TryGetValue(t.Attribute("index").Value, out tagdef)) {
                    tagset.Add(tagdef.Attribute("name").Value);
                }
            }
            _originalTags = tagset.ToArray();
        }

        internal bool IsDeleted {
            get {
                XAttribute recycleBinAtt = _page.Attribute("isInRecycleBin");

                return recycleBinAtt != null ? bool.Parse(recycleBinAtt.Value) : false;
            }
        }

        /// <summary>
        /// Get or set the unique ID of the OneNote Page
        /// </summary>
        internal string PageID { get; private set; }

        internal string[] PageTags {
            get {
                return _tags ?? _originalTags;
            }

            set {
                _tags = value;
            }
        }

        internal string Title {
            get {
                return _titleOE != null ? Regex.Replace(_titleOE.Value, "<[^<>]*>", "") : "Untitled page";
            }
        }

        internal static string[] ParseTags(string tags) {
            if (!string.IsNullOrEmpty(tags)) {
                // remove all HTML markup
                tags = Regex.Replace(tags, "<[^<>]+>", String.Empty);
                string[] parsedTags = tags.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                // normalize tags
                for (int i = 0; i < parsedTags.Length; i++) {
                    parsedTags[i] = parsedTags[i].Trim();
                }
                return parsedTags;
            }
            return new string[0];
        }

        /// <summary>
        /// Save all changes to the page to OneNote
        /// </summary>
        internal void Update() {
            if (_tags != null) {
                string[] savedTags = _tags;
                if (ApplyTagsToPage()) {
                    try {
                        _onenote.UpdatePage(_pageDoc, _lastModified);
                        _tags = null;
                    } catch (COMException ce) {
                        unchecked {
                            if (ce.ErrorCode == (int)0x80042010) { // try again after concurrent page modification
                                LoadOneNotePage();
                                PageTags = savedTags;
                                ApplyTagsToPage();
                                _onenote.UpdatePage(_pageDoc, _lastModified);
                                _tags = null;
                            } else {
                                throw;
                            }
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
        private void addElementToPage(XElement element, int sequence) {
            // locate a successor
            for (int i = sequence; i < ELEMENT_SEQUENCE.Length; i++) {
                XElement first = _page.Elements(_one.GetName(ELEMENT_SEQUENCE[i])).FirstOrDefault();
                if (first != null) {
                    first.AddBeforeSelf(element);
                    return;
                }
            }
            _page.Add(element);
        }

        /// <summary>
        /// Add tags to the page as specified by the <see cref="TagDisplay" /> enumeration.
        /// </summary>
        /// <returns><i>true</i> if a page update is needed; <i>false</i> otherwise</returns>
        private bool ApplyTagsToPage() {
            if (_tags == null) {
                return false; // can only apply tags once
            }

            bool specChanged = UpdateTagSpec();

            // get the new set of tags
            string newMetaContent = _meta != null ? _meta.Attribute("content").Value : String.Empty;

            XName tagName = _one.GetName("Tag");
            if (string.IsNullOrEmpty(newMetaContent)) {
                if (_pageTagsOE != null) { // no more tags - remove tag display
                    if (_pageTagsOE == _titleOE) { // in title tag
                        string markertagindex = _markerTagDef.Attribute("index").Value;
                        XElement markerTag = _titleOE.Elements(tagName).FirstOrDefault(t => t.Attribute("index").Value == markertagindex);
                        markerTag.Remove();
                    } else { // below title tags
                        _onenote.DeletePageContent(PageID, _pageTagsOE.Parent.Parent.Attribute("objectID").Value);
                        _pageTagsOE.Parent.Parent.Remove();
                        _pageTagsOE = null;
                    }
                }
                return true;
            }

            if (_pageTagsOE == null) { // make sure we update the page even if tags are unchanged
                specChanged = true;
            }
            XName tagdefName = _one.GetName("TagDef");
            if (specChanged) {  // some differences detected - must update
                string markerTagIndex = _markerTagDef.Attribute("index").Value; ;
                switch ((TagDisplay)Properties.Settings.Default.TagDisplay) {
                    case TagDisplay.BelowTitle:
                        // remove unwanted representation - in title tags
                        if (_pageTagsOE != null && _pageTagsOE == _titleOE) {
                            XElement tag = _pageTagsOE.Elements(tagName).FirstOrDefault(e => e.Attribute("index").Value == markerTagIndex);
                            if (tag != null) {
                                tag.Remove();
                            }
                            _pageTagsOE = null; // rebuild outline;
                        }

                        // update/create the tag outline
                        if (_pageTagsOE != null) { // remove all old <one:T> text elements, but leave the marker tag in place.
                            XName tName = _one.GetName("T");
                            foreach (XElement t in _pageTagsOE.Elements(tName).ToArray()) {
                                t.Remove();
                            }
                            // Add comma separated tags
                            _pageTagsOE.Add(new XElement(tName, newMetaContent));
                        } else { // build new outline for the tags
                                 // Create a style for the tags - if needed
                                 // <one:QuickStyleDef index="1"

                            // name="cite" fontColor="#595959" highlightColor="automatic"
                            // font="Calibri" fontSize="9"
                            XName styledefName = _one.GetName("QuickStyleDef");

                            IEnumerable<XElement> quickstyleDefs = _page.Elements(styledefName);
                            XElement tagStyle = quickstyleDefs.FirstOrDefault(d => d.Attribute("name").Value == Properties.Settings.Default.TagOutlineStyle_Name);

                            if (tagStyle == null) { // new style required
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
                            // create a tag outline
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
                                                            new XAttribute("z", "0")),
                                               new XElement(_one.GetName("Size"),
                                                            new XAttribute("width", "400"),
                                                            new XAttribute("height", "10"),
                                                            new XAttribute("isSetByUser", "true")),
                                               new XElement(_one.GetName("OEChildren"),
                                             (_pageTagsOE = new XElement(_one.GetName("OE"),
                                                                         new XAttribute("quickStyleIndex", tagStyle.Attribute("index").Value),
                                                                         new XElement(tagName,
                                                                                      new XAttribute("index", _markerTagDef.Attribute("index").Value),
                                                                                      new XAttribute("completed", "true")),
                                                                         new XElement(_one.GetName("T"), newMetaContent)))));
                            _page.Add(outline);
                        }
                        // turn off spell checking
                        _pageTagsOE.SetAttributeValue("lang", "yo");
                        break;

                    case TagDisplay.InTitle:
                        if (_pageTagsOE != null && _pageTagsOE != _titleOE) { // remove unwanted representation - below title tags
                            _onenote.DeletePageContent(PageID, _pageTagsOE.Parent.Parent.Attribute("objectID").Value);
                            _pageTagsOE.Parent.Parent.Remove();
                            _pageTagsOE = _titleOE;
                        }

                        // tag the title if needed
                        if (_titleOE.Elements(tagName).FirstOrDefault(e => markerTagIndex.Equals(e.Attribute("index").Value)) == null) {
                            _titleOE.AddFirst(new XElement(tagName,
                                                  new XAttribute("index", markerTagIndex),
                                                  new XAttribute("completed", "true")));
                        }
                        break;
                }
            }
            return specChanged;
        }

        private void LoadOneNotePage() {
            _pageTagsOE = null;
            _pageDoc = _onenote.GetPage(PageID);
            _one = _pageDoc.Root.GetNamespaceOfPrefix("one");
            _page = _pageDoc.Root;

            XName metaName = _one.GetName("Meta");
            _meta = _page.Elements(metaName).FirstOrDefault(m => m.Attribute("name").Value == META_NAME);

            _lastModified = DateTime.Parse(_page.Attribute("lastModifiedTime").Value);

            XName outlineName = _one.GetName("Outline");

            // find the various tag definitions <one:TagDef> used to add textual tags to
            // the page title<one:TagDef index="0" name="Tags" type="23" symbol="26" />
            _markerTagDef = _page.Elements(_one.GetName("TagDef")).FirstOrDefault(d => ("26".Equals(d.Attribute("symbol").Value)
                                                                                       && ("99".Equals(d.Attribute("type").Value)
                                                                                           || (MarkerTagname.Equals(d.Attribute("name").Value) && "23".Equals(d.Attribute("type").Value))
                                                                                          )
                                                                                       ));
            // For performance reasons we are going to delete all outlines not related to tags
            // Note: Page updates will actually leave those removed outlines on the page.
            List<XElement> outlinesToDelete = new List<XElement>();

            // find <one:Outline> containing page tags.
            if (_markerTagDef != null) {
                string defindex = _markerTagDef.Attribute("index").Value;
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
                foreach (var outline in _page.Elements(outlineName)) {
                    // the outline we are looking for has one <one:OEChildren> element with
                    // one <one:OE> element containing a <one:Tag> with the given index
                    if (_pageTagsOE == null) {
                        XElement OEChildren = outline.Element(_one.GetName("OEChildren"));
                        if (OEChildren != null) {
                            XElement OE = OEChildren.Element(_one.GetName("OE"));
                            if (OE != null) {
                                XElement tag = (from t in OE.Elements(_one.GetName("Tag"))
                                                where t.Attribute("index").Value == defindex
                                                select t).FirstOrDefault();
                                if (tag != null) { // found the outline with page tags, get the text element containing the tags
                                    _pageTagsOE = OE;
                                    // delete the Indents as they tend to cause errors
                                    XElement indents = outline.Element(_one.GetName("Indents"));
                                    if (indents != null) {
                                        indents.Remove();
                                    }
                                    continue;
                                }
                            }
                        }
                    }
                    outlinesToDelete.Add(outline); // enroll for deletion
                }
            } else { // just record all outline elements for deletion
                foreach (var outline in _page.Elements(outlineName)) {
                    outlinesToDelete.Add(outline);
                }
            }

            // delete outlines to make updating the page as fast as possible, knowing that
            // the update method will keep these outlines actually on the page.
            foreach (XElement outline in outlinesToDelete) {
                outline.Remove();
            }

            XName titleName = _one.GetName("Title");
            XElement title = _page.Element(titleName);
            if (title == null) {
                title = new XElement(titleName, _titleOE = new XElement(_one.GetName("OE")));
                addElementToPage(title, TITLE_IDX);
            } else {
                _titleOE = title.Element(_one.GetName("OE"));
                if (_markerTagDef != null && "99".Equals(_markerTagDef.Attribute("type").Value)) {
                    _pageTagsOE = _titleOE;
                }
            }
        }

        private bool UpdateTagSpec() {
            bool specChanged = false;
            // collect all tag definitions
            XName tagdefName = _one.GetName("TagDef");
            IEnumerable<XElement> tagDefs = _page.Elements(tagdefName);

            int nextTagDefIndex = tagDefs.Count(); // next index for creating new tags
                                                   // add new tags to the page title and remove obsolete ones
                                                   // <one:Title lang="de">
                                                   //  <one:OE objectID="{9A0ACA13-6D63-4137-8821-5D7C5408BB6C}{15}{B0}" lastModifiedTime="2013-12-08T14:08:11.000Z" quickStyleIndex="0" author="Peter Ernst" authorInitials="PE" lastModifiedBy="Peter Ernst" lastModifiedByInitials="PE" creationTime="2013-12-08T14:08:11.000Z">
                                                   //    <one:Tag index="0" completed="true" creationDate="2013-12-06T20:31:43.000Z" completionDate="2013-12-06T20:31:43.000Z" />
                                                   //    <one:Tag index="1" completed="true" creationDate="2013-12-06T20:34:05.000Z" completionDate="2013-12-06T20:34:05.000Z" />
                                                   //    <one:Tag index="2" completed="true" creationDate="2013-12-06T20:35:41.000Z" completionDate="2013-12-06T20:35:41.000Z" />
                                                   //    <one:T><![CDATA[Test Addin ]]></one:T>
                                                   //  </one:OE>
                                                   //</one:Title>

            // Locate tag definitions for existing page tags and record them:
            // <one:TagDef index="0" name="Test Tag 1" type="0" symbol="0" />
            // <one:TagDef index="1" name="Test Tag 2" type="1" symbol="0" />
            var tagnameToTagdefMap = new Dictionary<string, XElement>();
            int redundantTagCount = 0;
            foreach (XElement tagdef in tagDefs.Where(d => d.Attribute("symbol").Value == "0"
                                                          && _originalTags.Contains(d.Attribute("name").Value))) {
                string tagname = tagdef.Attribute("name").Value;
                if (tagnameToTagdefMap.ContainsKey(tagname)) { // another tag with that name already exists.
                                                               // Give that tag a unique
                                                               // name, so that if can be
                                                               // cleaned up later
                    tagname = string.Format("{0}@#|{1}", tagname, redundantTagCount++);
                }
                tagnameToTagdefMap[tagname] = tagdef;
                // make sure type is equal to index so that type is unique
                tagdef.Attribute("type").Value = tagdef.Attribute("index").Value;
                // remove any color attribute so that the title can show its original color
                if (tagdef.Attribute("fontColor") != null) {
                    tagdef.Attribute("fontColor").Remove();
                }
            }
            XName tagName = _one.GetName("Tag");
            // add new tag definitions, if needed
            foreach (string tag in PageTags) {
                XElement tagdef;
                string strIndex;
                if (tagnameToTagdefMap.TryGetValue(tag, out tagdef)) {
                    strIndex = tagdef.Attribute("index").Value;
                } else { // create a new definition for this tag
                    strIndex = (nextTagDefIndex++).ToString(CultureInfo.InvariantCulture);
                    tagdef = new XElement(tagdefName, new XAttribute("index", strIndex),
                                                      new XAttribute("name", tag),
                                                      new XAttribute("type", strIndex),
                                                      new XAttribute("symbol", "0"));
                    _page.AddFirst(tagdef);
                }

                // tag the title with an invisible tag if needed

                XElement titleTag = _titleOE.Elements(tagName).FirstOrDefault(t => t.Attribute("index").Value == strIndex);

                if (titleTag == null) {
                    _titleOE.AddFirst(new XElement(tagName,
                                                  new XAttribute("index", strIndex),
                                                  new XAttribute("completed", "true")));
                    specChanged = true;
                }

                tagnameToTagdefMap.Remove(tag); // remove used tag
            }

            // remove unused tags from the title; this also automatically removes their
            // definitions on next page update.
            // TODO: avoid quadratic lookup
            foreach (XElement td in tagnameToTagdefMap.Values) {
                string strIndex = td.Attribute("index").Value;
                XElement tag = _titleOE.Elements(tagName).FirstOrDefault(t => t.Attribute("index").Value == strIndex);
                if (tag != null) {
                    tag.Remove();
                    specChanged = true;
                }
            }

            string strTags = string.Join(", ", PageTags);

            // create the <one:Meta> element for page tags, if needed
            if (_tags.Length > 0) { // we have tags to set
                if (_meta == null) {
                    _meta = new XElement(_one.GetName("Meta"),
                                    new XAttribute("name", META_NAME));
                    addElementToPage(_meta, META_IDX);
                    specChanged = true;
                }
                _meta.SetAttributeValue("content", strTags);
            } else {
                if (_meta != null) { // we cannot remove the meta element, so we just set it to the empty string.
                    _meta.SetAttributeValue("content", string.Empty);
                    specChanged = true;
                }
            }

            // create or update the marker tag definition
            if (_markerTagDef != null) { // existing tag definition recycling
                switch ((TagDisplay)Properties.Settings.Default.TagDisplay) {
                    case TagDisplay.BelowTitle:
                        if (!"23".Equals(_markerTagDef.Attribute("type").Value)) {
                            _markerTagDef.SetAttributeValue("type", "23");
                            specChanged = true;
                        }
                        if (!MarkerTagname.Equals(_markerTagDef.Attribute("type").Name)) {
                            _markerTagDef.SetAttributeValue("name", MarkerTagname);
                            specChanged = true;
                        }

                        break;

                    case TagDisplay.InTitle:
                        if (!"99".Equals(_markerTagDef.Attribute("type").Value)) {
                            _markerTagDef.SetAttributeValue("type", "99");
                            specChanged = true;
                        }
                        if (!strTags.Equals(_markerTagDef.Attribute("name").Value)) {
                            _markerTagDef.SetAttributeValue("name", strTags);
                            specChanged = true;
                        }
                        break;
                }
            } else { // create a new tag definition
                string strIndex = nextTagDefIndex.ToString(CultureInfo.InvariantCulture);
                switch ((TagDisplay)Properties.Settings.Default.TagDisplay) {
                    case TagDisplay.BelowTitle:
                        _markerTagDef = new XElement(tagdefName,
                                            new XAttribute("index", strIndex),
                                            new XAttribute("symbol", "26"),
                                            new XAttribute("name", MarkerTagname),
                                            new XAttribute("type", "23"));

                        break;

                    case TagDisplay.InTitle:
                        _markerTagDef = new XElement(tagdefName,
                                            new XAttribute("index", strIndex),
                                            new XAttribute("symbol", "26"),
                                            new XAttribute("name", strTags),
                                            new XAttribute("type", "99"));
                        break;
                }
                _page.AddFirst(_markerTagDef);
                specChanged = true;
            }
            return specChanged;
        }
    }
}