// Author: WetHat | (C) Copyright 2013 - 2017 WetHat Lab, all rights reserved
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using WetHatLab.OneNote.TaggingKit.common;
using WetHatLab.OneNote.TaggingKit.HierarchyBuilder;

namespace WetHatLab.OneNote.TaggingKit.PageBuilder
{
    /// <summary>
    /// Enumeration of ways to display tags on a Onenote Page
    /// </summary>
    public enum TagDisplay
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

    /// <summary>Sequence positions of page structure elements</summary>
    /// <remarks>
    /// <code lang="xml">
    /// // Sequence of elements below the page tag
    /// <xsd:element name="TagDef" type="TagDef" minOccurs="0" maxOccurs="unbounded"/>[
    /// <xsd:element name="QuickStyleDef" type="QuickStyleDef" minOccurs="0" maxOccurs="unbounded"/>
    /// <xsd:element name="XPSFile" type="XPSFile" minOccurs="0" maxOccurs="unbounded"/>
    /// <xsd:element name="Meta" type="Meta" minOccurs="0" maxOccurs="unbounded"/>
    /// <xsd:element name="MediaPlaylist" type="MediaPlaylist" minOccurs="0"/>
    /// <xsd:element name="MeetingInfo" type="MeetingInfo" minOccurs="0"/>
    /// <xsd:element name="PageSettings" type="PageSettings" minOccurs="0"/>
    /// <xsd:element name="Title" type="Title" minOccurs="0"/>
    /// </code>
    /// </remarks>
    public enum PageSchemaPosition
    {
        /// <summary>
        ///     Position of the first tag definition element of a OneNote page.
        /// </summary>
        TagDef = 0,
        /// <summary>
        ///     Position of the first style definition element of a OneNote page.
        /// </summary>
        QuickStyleDef = 1,

        XPSFile = 2,

        /// <summary>
        ///     Position of the first _Meta_ element of a OneNote page.
        /// </summary>
        Meta = 3,

        MediaPlaylist = 4,

        MeetingInfo = 5,

        /// <summary>
        ///     Position of the page format settings.
        /// </summary>
        PageSettings = 6,

        /// <summary>
        ///     Position of the _Title_ element on a OneNote page.
        /// </summary>
        Title = 7,

        /// <summary>
        ///     Position of the firswwt _Outline_ element of a OneNote page.
        /// </summary>
        Outline = 8
    }

    /// <summary>
    ///     Local proxy of a OneNote page.
    /// </summary>
    /// <remarks>
    ///     Supports:
    ///     <list type="bullet">
    ///     <item>tag related operations</item>
    ///     <item>enbedding and updating saved searches.</item>
    ///     </list>
    /// </remarks>
    public class OneNotePage : PageObjectBase {
        private static readonly Regex _hashtag_matcher = new Regex(@"(?<=^|[^\w#-_]|[({\['])(?>#[^\W\d_][\w-_]*)(?![#({\[/~])", RegexOptions.Compiled);
        private static readonly Regex _hashtag_matcherRTL = new Regex(@"(?<=^|[^\w#)}\]])(?>[\w-_]*[^\W\d_]#)(?=$|[^\w#-_~]|[)}\]'])", RegexOptions.Compiled);
        
        /// <summary>
        /// The sequence of structure elements in which elements have to
        /// appear on a OneNote page.
        /// </summary>
        /// <remarks>
        /// The schema for stucture elements on a OneNote page is defined as:
        /// <code lang="xml">
        /// // Sequence of elements below the page tag
        /// <xsd:element name="TagDef" type="TagDef" minOccurs="0" maxOccurs="unbounded"/>[
        /// <xsd:element name="QuickStyleDef" type="QuickStyleDef" minOccurs="0" maxOccurs="unbounded"/>
        /// <xsd:element name="XPSFile" type="XPSFile" minOccurs="0" maxOccurs="unbounded"/>
        /// <xsd:element name="Meta" type="Meta" minOccurs="0" maxOccurs="unbounded"/>
        /// <xsd:element name="MediaPlaylist" type="MediaPlaylist" minOccurs="0"/>
        /// <xsd:element name="MeetingInfo" type="MeetingInfo" minOccurs="0"/>
        /// <xsd:element name="PageSettings" type="PageSettings" minOccurs="0"/>
        /// <xsd:element name="Title" type="Title" minOccurs="0"/>
        /// </code>
        /// </remarks>
        static readonly string[] PositionNames = new string[] { "TagDef", "QuickStyleDef", "XPSFile", "Meta", "MediaPlaylist", "MeetingInfo", "PageSettings", "Title", "Outline" };

        /// <summary>
        ///     Anchor XML element marking locations of page structure elements which need to be at
        ///     schema defined positions.
        /// </summary>
        /// <remarks>
        ///     The correct sequence of elements is described by the <see cref="PageSchemaPosition"/>
        ///     enumeration.
        /// </remarks>
        static readonly XElement[] PageAnchors = new XElement[] { null, null, null, null, null, null, null, null, null };
        /// <summary>
        /// Get the style definitions for this page.
        /// </summary>
        public QuickStyleDefCollection QuickStyleDefinitions { get; }

        MetaCollection _meta; // Page Meta information
        TagDefCollection _tagdef; // OneNote tag definitions.

        /// <summary>
        ///     Get or set date and time this page was last modified.
        /// </summary>
        DateTime LastModified { get; set; }

        /// <summary>
        ///     Get the page format settings.
        /// </summary>
        PageSettings Settings {get; set;}
        /// <summary>
        /// Get the OneNote application object.
        /// </summary>
        public OneNoteProxy OneNoteApp { get;  }

        /// <summary>
        /// Get the XML document of the OneNote page
        /// </summary>
        XDocument Document { get; set; }

        /// <summary>
        /// Get the saved seatches on this page.
        /// </summary>
        public OESavedSearchCollection SavedSearches;
        OETaglist _belowTitleTags;

        /// <summary>
        /// Get the proxy object for the 'one:Title' element of the OneNote page
        /// document.
        /// </summary>
        public Title Title { get; private set; }

        /// <summary>
        ///     The the first XML element at a schema position.
        /// </summary>
        /// <param name="p">
        ///     The schema position.
        /// </param>
        /// <returns>
        ///     THe XML element at the given position; `null` if element is
        ///     at the given position.
        /// </returns>
        public XElement this[PageSchemaPosition p] {
            get {
                return PageAnchors[(int)p];
            }
        }

        /// <summary>
        /// Initialize a proxy object for an existing OneNote page.
        /// </summary>
        /// <param name="onenoteApp">The OneNote application object.</param>
        /// <param name="pageID">The page ID.</param>
        /// <param name="defaultTitle">
        ///     The default title to use if the page does not already have a title.
        /// </param>
        internal OneNotePage(OneNoteProxy onenoteApp, string pageID, string defaultTitle = "")
            : base(onenoteApp.GetPage(pageID).Root) {
            OneNoteApp = onenoteApp;
            PageID = pageID;
            Document = Element.Document; // get the page's XML document
            LastModified = DateTime.Parse(Element.Attribute("lastModifiedTime").Value);

            // Determine the overall page structure
            for (PageSchemaPosition p = PageSchemaPosition.TagDef; p <= PageSchemaPosition.Outline; p++) {
                var firstElement = Element.Element(GetName(p.ToString()));
                if (firstElement != null) {
                    PageAnchors[(int)p] = firstElement;
                }
            }

            // process page format settings
            if (PageAnchors[(int)PageSchemaPosition.PageSettings] == null) {
                // make a settings object
                Settings = new PageSettings(this);
            } else {
                Settings = new PageSettings(this, PageAnchors[(int)PageSchemaPosition.PageSettings]);
            }

            // process style definitions
            QuickStyleDefinitions = new QuickStyleDefCollection(this);
            _meta = new MetaCollection(this);
            _tagdef = new TagDefCollection(this);

            if (PageAnchors[(int)PageSchemaPosition.Title] == null) {
                // a title is required for tagging
                Title = new Title(this, defaultTitle);
            } else {
                Title = new Title(this, PageAnchors[(int)PageSchemaPosition.Title]);
                // inspect the title tags an mark collect the managed oage tags
                Tags.UnionWith(from tt in Title.Tags
                               let td = _tagdef[tt.Index]
                               where td.Tag != null && td.Tag.TagType <= PageTagType.HashTag
                               select td.Tag);
                // use the in-title tag to define even more page tags.
                if (_tagdef.InTitleMarkerDef != null) {
                    Tags.UnionWith(from t in PageTagSet.Parse(_tagdef.InTitleMarkerDef.Name, TagFormat.AsEntered)
                                   where t.TagType <= PageTagType.HashTag
                                   select t);
                }
            }

            // make sure all tags recorded in the page's meta information are defined too
            Tags.UnionWith(from t in PageTagSet.Parse(_meta.PageTags,TagFormat.AsEntered)
                           where t.TagType <= PageTagType.HashTag
                           select t);

            SavedSearches = new OESavedSearchCollection(this);

            // For performance reasons we are going to delete all outlines not related to tags
            // or saved searches.
            // Note: Page updates will actually leave those removed outlines on the page.
            List<XElement> outlinesToDelete = new List<XElement>();
            XName outlineName = GetName("Outline");

            // For performance reasons we avoid Xpath!
            foreach (var outline in Element.Elements(outlineName).ToList()) {
                XElement OEChildren = outline.Element(GetName("OEChildren"));
                if (OEChildren != null) {
                    // further inspect that outline
                    if (_tagdef.BelowTitleMarkerDef != null && _belowTitleTags == null) {
                        // there is a below title taglist on the page
                        // the outline we are looking for has one <one:OEChildren> element with
                        // one <one:OE> element containing a <one:Tag> with the given index
                        // The tag outline element lookis like this
                        // <one:Outline author="Peter Ernst" authorInitials="PE" lastModifiedBy="Peter Ernst" lastModifiedByInitials="PE" objectID="{E470786C-A904-4E9F-AC3B-0D9F36B6FC54}{14}{B0}" lastModifiedTime="2013-12-06T16:04:48.000Z">
                        //   <one:Position x="236.249984741211" y="42.1500015258789" z="0" />
                        //   <one:Size width="90.8847274780273" height="10.9862976074219" />
                        //   <one:OEChildren>
                        //     <one:OE objectID="{E470786C-A904-4E9F-AC3B-0D9F36B6FC54}{15}{B0}" lastModifiedTime="2013-12-06T16:04:48.000Z" quickStyleIndex="1" creationTime="2013-12-06T16:03:50.000Z">
                        //       <one:Tag index="0" completed="true" creationDate="2013-12-06T15:55:59.000Z" completionDate="2013-12-06T15:55:59.000Z" />
                        //       <one:T><![CDATA[Gdfgdf, sdfdsf]]></one:T>
                        //     </one:OE>
                        //   </one:OEChildren>
                        // </one:Outline>

                        XElement oelement = OEChildren.Element(GetName("OE"));
                        if (oelement != null) {
                            // make a proxy for that
                            var oe = new OE(oelement);
                            if (oe.Tags.Contains(_tagdef.BelowTitleMarkerDef)) {
                                _belowTitleTags = new OETaglist(oe);
                                // collect these tags too
                                //Tags.UnionWith(from t in new PageTagSet(_belowTitleTags.Taglist, TagFormat.AsEntered)
                                //               where t.TagType <= PageTagType.HashTag
                                //               select t);
                                // delete the Indents as they tend to cause errors
                                XElement indents = outline.Element(GetName("Indents"));
                                if (indents != null) {
                                    indents.Remove();
                                }
                                continue; // keep this outline
                            }
                        }
                    } else if (_tagdef.SavedSearchMarkerDef != null && SavedSearches.Add(outline,_tagdef.SavedSearchMarkerDef)) {
                        // saved search could be added - keep that outline
                        continue;
                    }
                    if (Properties.Settings.Default.MapHashTags) {
                        // make sure all hashtags in that outline are defined
                        foreach (var oe in outline.Descendants(GetName("OE"))) {
                            bool rtl = false;
                            XAttribute rtlAtt = oe.Attribute("RTL");
                            if (rtlAtt != null) {
                                rtl = bool.Parse(rtlAtt.Value);
                            }
                            else {
                                rtl = Settings.IsRTL;
                            }
                            foreach (var t in oe.Descendants(GetName("T"))) {
                                // remove some non-sensical tags from the text
                                string txt = OETaglist.HTMLtag_matcher.Replace(t.Value, string.Empty);
                                if (rtl) {
                                    _importedTags.UnionWith(from Match m in _hashtag_matcherRTL.Matches(txt)
                                                            where m.Value.Length > 1
                                                            select new PageTag(m.Value.Trim(), PageTagType.ImportedHashTag));
                                } else {
                                    _importedTags.UnionWith(from Match m in _hashtag_matcher.Matches(txt)
                                                            where m.Value.Length > 1
                                                            select new PageTag(m.Value.Trim(), PageTagType.ImportedHashTag));
                                }
                            }
                        }
                    }
                }
                outline.Remove();
            }
        }

        /// <summary>
        /// Define a OneNote process tag.
        /// </summary>
        /// <param name="name">Tag name</param>
        /// <param name="cls">Tag Classification</param>
        /// <returns></returns>
        public TagDef DefineProcessTag(string name, TagProcessClassification cls) => _tagdef.DefineProcessTag(name,cls);

        /// <summary>
        /// Determine if this page is in the recycle bin.
        /// </summary>
        public bool IsDeleted {
            get {
                XAttribute recycleBinAtt = Element.Attribute("isInRecycleBin");
                return recycleBinAtt != null && "true".Equals(recycleBinAtt.Value);
            }
        }

        /// <summary>
        /// Get or set the unique ID of the OneNote Page.
        /// </summary>
        internal string PageID { get; private set; }

        /// <summary>
        /// Get/set the page tags.
        /// </summary>
        /// <value>Set of user created page tags.</value>
        public PageTagSet Tags { get; set; } = new PageTagSet();

        PageTagSet _importedTags = new PageTagSet();
        /// <summary>
        /// Save all changes to the page to OneNote.
        /// </summary>
        /// <param name="sync">
        ///     `true` if this is a request to update import tags
        ///     and saved searches.
        /// </param>
        internal void Update(bool sync = false) {
            if (ApplyTagsToPage() || (!SavedSearches.Empty && sync)) {
                try {
                    if (sync) {
                        SavedSearches.Update(); // expensive!
                    }
                    OneNoteApp.UpdatePage(Document, LastModified);
                } catch (COMException ce) {
                    unchecked {
                        switch ((uint)ce.ErrorCode) {
                            case 0x80042010: // concurrent page modification
                                TraceLogger.Log(TraceCategory.Error(), "The last modified date does not match. Concurrent page modification: {0}\n Rescheduling tagging job.", ce.Message);
                                break;

                            case 0x80042030: // blocked by modal dialog
                                TraceLogger.ShowGenericErrorBox(Properties.Resources.TaggingKit_Blocked, ce);
                                break;

                            default:
                                throw;
                        }
                    }
                    // attempt one retry with an updated modify-timestamp
                    PageNode pg = new PageNode(OneNoteApp.GetPage(PageID).Root, null);
                    OneNoteApp.UpdatePage(Document, pg.LastModified);
                }
                if (_belowTitleTags != null
                        && ((TagDisplay)Properties.Settings.Default.TagDisplay == TagDisplay.InTitle
                            || _tagdef.DefinedPageTags.FirstOrDefault() == null)) {
                    // delete obsolete outline with tag list
                    // the corresponding outline too
                    OneNoteApp.DeletePageContent(PageID, _belowTitleTags.Element.Parent.Parent.Attribute("objectID").Value);
                }
            }
        }

        /// <summary>
        ///     Add a new page structure element at the correct location to a
        ///     OneNote page XML document.
        /// </summary>
        /// <remarks>
        ///     Makes use of an internal lookup table containing anchor elements
        ///     known to be at certain schema positions.
        /// </remarks>
        /// <param name="obj">
        ///     The proxy object containing the element to add to the page.
        /// </param>
        /// <param name="position">
        ///     The page position of elements of this type according to the page schema.
        /// </param>
        public void Add(PageStructureObjectBase obj,PageSchemaPosition position) {
            XElement anchor = PageAnchors[(int)position];
            if (anchor == null) {
                // need to go searching for the next known element
                for (int i = (int)position+1; i < PositionNames.Length; i++) {
                    anchor = PageAnchors[i];
                    if (anchor != null) {
                        break; // got one
                    }
                }
            }

            if (anchor != null) {
                anchor.AddBeforeSelf(obj.Element);
            } else {
                Element.Add(obj.Element);
            }
            PageAnchors[(int)position] = obj.Element; // make sure this is the first element of its kind
        }
        /// <summary>
        /// Add tags to the page as specified by the <see cref="TagDisplay" /> enumeration.
        /// </summary>
        /// <returns><i>true</i> if a page update is needed; <i>false</i> otherwise</returns>
        private bool ApplyTagsToPage() {
            // determine all tags the title currently has
            TagCollection titletags = Title.Tags;
            // make sure we are not hihacking symbol less OneNote tags
            foreach (var td in _tagdef.DefinedPageTags) {
                if (!titletags.Contains(td)) {
                    // this is a symbol-less OneNote tag somwhere else on the page
                    td.Tag = null;
                }
            }
            // get all the tags currently on the title
            var titletagset = new HashSet<TagDef>(from tag in titletags select _tagdef[tag.Index]);

            // import OneNote tags (if enabled)
            if (Properties.Settings.Default.MapOneNoteTags) {
                _importedTags.UnionWith(from td in _tagdef
                                        where td.ProcessClassification == TagProcessClassification.OneNoteTag
                                        select new PageTag(td.Name, PageTagType.ImportedOneNoteTag));
            }

            // combine the managed tags and the import tags to obtain all the
            // tags that need to be on the page
            var pagetagset = new PageTagSet();
            pagetagset.UnionWith(Tags);
            pagetagset.UnionWith(_importedTags);

            // define all the tags
            _tagdef.DefinePageTags(pagetagset);
            bool specChanged = _tagdef.IsModified;

            // ... and make sure these tags are on the title too
            titletagset.UnionWith(_tagdef.DefinedPageTags);
            if (specChanged) {
                // Get rid of obsolete definitions
                titletagset = new HashSet<TagDef>(from TagDef td in titletagset where !td.IsDisposed select td);
            }
            // update the meta information of the page
            _meta.PageTags = pagetagset.ToString();
            specChanged |= _meta.IsModified;

            switch ((TagDisplay)Properties.Settings.Default.TagDisplay) {
                case TagDisplay.InTitle:
                    if (_belowTitleTags != null && !string.IsNullOrEmpty(_belowTitleTags.ElementId)) {
                        // The now also obsolete outline containing the tag list will
                        // be removed from the page in 'Update' to avoid concurrent
                        // page modification.
                        specChanged = true;
                    }

                    if (pagetagset.IsEmpty) {
                        if (_tagdef.InTitleMarkerDef != null) {
                            titletagset.Remove(_tagdef.InTitleMarkerDef);
                            _tagdef.InTitleMarkerDef.Dispose();
                            specChanged = true;
                        }
                    } else if (_tagdef.InTitleMarkerDef == null) {
                        TagDef inTitleMarker = _tagdef.DefineProcessTag(
                                                   pagetagset.ToString(),
                                                   TagProcessClassification.InTitleMarker);
                        titletagset.Add(_tagdef.InTitleMarkerDef);
                        specChanged = true;
                    } else {
                        string oldname =  _tagdef.InTitleMarkerDef.Name;
                        string newname = pagetagset.ToString();
                        if (!oldname.Equals(newname)) {
                            // tag display changed
                            _tagdef.InTitleMarkerDef.Name = newname;
                            specChanged = true;
                        }
                        titletagset.Add(_tagdef.InTitleMarkerDef);
                    }
                    break;
                case TagDisplay.BelowTitle:
                    if (_tagdef.InTitleMarkerDef != null) {
                        // cleanup obsolete tag display
                        titletagset.Remove(_tagdef.InTitleMarkerDef);
                        _tagdef.InTitleMarkerDef.Dispose();
                        specChanged = true;
                    }

                    if (pagetagset.IsEmpty) {
                        if (_belowTitleTags != null) {
                            // Enable removal of the taglist outline by the
                            // `Update` method.
                            specChanged = true;
                        }
                    } else if (_belowTitleTags == null) {
                        TagDef belowTitleMarker = _tagdef.DefineProcessTag(TagDef.sLegacyBelowTitleMarkerName, TagProcessClassification.BelowTitleMarker);

                        // create a new outline for below-title tags
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
                        XElement outline = new XElement(GetName("Outline"),
                                               new XElement(GetName("Position"),
                                                   new XAttribute("x", "236"),
                                                   new XAttribute("y", "43"),
                                                   new XAttribute("z", "0")),
                                               new XElement(GetName("Size"),
                                                   new XAttribute("width", "400"),
                                                   new XAttribute("height", "10"),
                                                   new XAttribute("isSetByUser", "true")),
                                               new XElement(GetName("OEChildren"),
                                (_belowTitleTags = new OETaglist(Namespace, pagetagset.ToString(", "), QuickStyleDefinitions.TagOutlineStyleDef)).Element));
                        Element.Add(outline);
                        _belowTitleTags.Tags.Add(belowTitleMarker);
                        specChanged = true;
                    } else {
                        // check if the below title tag display needs update
                        if (!specChanged) {
                            var displayedTags = new PageTagSet(_belowTitleTags.Taglist, TagFormat.AsEntered);
                            specChanged = displayedTags.Count != pagetagset.Count;
                            if (!specChanged) {
                                displayedTags.ExceptWith(pagetagset);
                                specChanged = !displayedTags.IsEmpty;
                            }
                        }
                        if (specChanged) {
                            _belowTitleTags.Taglist = pagetagset.ToString(", ");
                            _belowTitleTags.QuickStyle = QuickStyleDefinitions.TagOutlineStyleDef;
                        }
                    }
                    break;
            }

            if (!specChanged) {
                // make sure the title tags are up-to-date
                if (titletagset.Any((td) => !titletags.Contains(td))) {
                    titletags.Tags = titletagset;
                    specChanged = true;
                }
            } else {
                titletags.Tags = titletagset;
            }

            return specChanged || _tagdef.IsModified || QuickStyleDefinitions.IsModified;
        }
    }
}