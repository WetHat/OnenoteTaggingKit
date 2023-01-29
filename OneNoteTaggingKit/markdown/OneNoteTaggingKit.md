# OneNoteTaggingKit assembly

## WetHatLab.OneNote.TaggingKit namespace

| public type | description |
| --- | --- |
| class [ConnectTaggingKitAddin](./WetHatLab.OneNote.TaggingKit/ConnectTaggingKitAddin.md) | OneNote application connector. |
| class [OneNoteProxy](./WetHatLab.OneNote.TaggingKit/OneNoteProxy.md) | Proxy class to make method calls into the OneNote application object which are protected against recoverable errors and offer a tagging specific API. |
| enum [SearchScope](./WetHatLab.OneNote.TaggingKit/SearchScope.md) | Enumeration of scopes to search for pages |

## WetHatLab.OneNote.TaggingKit.common namespace

| public type | description |
| --- | --- |
| class [EventAdapter](./WetHatLab.OneNote.TaggingKit.common/EventAdapter.md) | Adapter class to facilitate raising events in a given tread context. |
| class [FilterableTagsSource&lt;T&gt;](./WetHatLab.OneNote.TaggingKit.common/FilterableTagsSource-1.md) | An observable list of tag related view models which can be filtered and highlighted by applying search criteria. |
| interface [IKeyedItem&lt;T&gt;](./WetHatLab.OneNote.TaggingKit.common/IKeyedItem-1.md) | Contract for classes which want to provide a unique key suitable for hashing |
| interface [IObservableTagList](./WetHatLab.OneNote.TaggingKit.common/IObservableTagList.md) | Contract for read-only, observable lists of tag models. |
| interface [ISortableKeyedItem&lt;TSort,TKey&gt;](./WetHatLab.OneNote.TaggingKit.common/ISortableKeyedItem-2.md) | Contract for classes which want to provide keys suitable for hashing and sorting |
| interface [ITagsAndPages](./WetHatLab.OneNote.TaggingKit.common/ITagsAndPages.md) | Contract for classes providing colelctions of pages and tags on them. |
| class [KnownTagsSource&lt;T&gt;](./WetHatLab.OneNote.TaggingKit.common/KnownTagsSource-1.md) | An observable list of tags which are already known. |
| enum [NotifyDictionaryChangedAction](./WetHatLab.OneNote.TaggingKit.common/NotifyDictionaryChangedAction.md) | Classification of changes to a [`ObservableDictionary`](./WetHatLab.OneNote.TaggingKit.common/ObservableDictionary-2.md) instance. |
| class [NotifyDictionaryChangedEventArgs&lt;TKey,TValue&gt;](./WetHatLab.OneNote.TaggingKit.common/NotifyDictionaryChangedEventArgs-2.md) | Event details describing to details of a changes to instances of [`ObservableDictionary`](./WetHatLab.OneNote.TaggingKit.common/ObservableDictionary-2.md) |
| delegate [NotifyDictionaryChangedEventHandler&lt;TKey,TValue&gt;](./WetHatLab.OneNote.TaggingKit.common/NotifyDictionaryChangedEventHandler-2.md) | delegate to handle change events in instances of [`ObservableDictionary`](./WetHatLab.OneNote.TaggingKit.common/ObservableDictionary-2.md) |
| class [ObservableDictionary&lt;TKey,TValue&gt;](./WetHatLab.OneNote.TaggingKit.common/ObservableDictionary-2.md) | A dictionary which notifies subscribed listeners about content changes. |
| class [ObservableObject](./WetHatLab.OneNote.TaggingKit.common/ObservableObject.md) | Base class for objects which expose observable properties. |
| class [ObservableSortedList&lt;TSort,TKey,TValue&gt;](./WetHatLab.OneNote.TaggingKit.common/ObservableSortedList-3.md) | An observable, sorted collection of items having sortable keys. |
| class [ObservableTagList&lt;T&gt;](./WetHatLab.OneNote.TaggingKit.common/ObservableTagList-1.md) | A generic observable list of sorted page tag models. |
| class [PageTag](./WetHatLab.OneNote.TaggingKit.common/PageTag.md) | Definition of page level tags which are recognized by the Tagging Kit. |
| class [PageTagSet](./WetHatLab.OneNote.TaggingKit.common/PageTagSet.md) | A consolidated set of page tags. |
| enum [PageTagType](./WetHatLab.OneNote.TaggingKit.common/PageTagType.md) | Enumeration of types of Tagging Kit recognized page tags which appear on OneNote pages. |
| enum [TagFormat](./WetHatLab.OneNote.TaggingKit.common/TagFormat.md) | Tag formatting options. |
| class [TagInputEventArgs](./WetHatLab.OneNote.TaggingKit.common/TagInputEventArgs.md) | Event details for a `TagInput or TagInput /&gt; event. |
| delegate [TagInputEventHandler](./WetHatLab.OneNote.TaggingKit.common/TagInputEventHandler.md) | Handler for the TagInput or TagInput event. |
| class [TagPageSet](./WetHatLab.OneNote.TaggingKit.common/TagPageSet.md) | The set of pages which have a particular tag in their &lt;one:Meta name="TaggingKit.PageTags" ...&gt; meta element. |
| class [TagsAndPages](./WetHatLab.OneNote.TaggingKit.common/TagsAndPages.md) | Observable collections of OneNote pages and the tags on them. |
| class [TagsAndPagesBase](./WetHatLab.OneNote.TaggingKit.common/TagsAndPagesBase.md) | Base class for collections of pages and the tags found on them. |
| struct [TextFragment](./WetHatLab.OneNote.TaggingKit.common/TextFragment.md) | Representation of a fragment of text which does or does not match a pattern |
| class [TextSplitter](./WetHatLab.OneNote.TaggingKit.common/TextSplitter.md) | Split text at pattern matches. |

## WetHatLab.OneNote.TaggingKit.common.ui namespace

| public type | description |
| --- | --- |
| class [FilterableTagModel](./WetHatLab.OneNote.TaggingKit.common.ui/FilterableTagModel.md) | A basic implementation of a view model for tags which can be filtered based on a pattern. |
| interface [ITagModel](./WetHatLab.OneNote.TaggingKit.common.ui/ITagModel.md) | Interface to be used by designer model. |
| class [ScopeChangedEventArgs](./WetHatLab.OneNote.TaggingKit.common.ui/ScopeChangedEventArgs.md) | Event details for the [`ScopeChanged`](./WetHatLab.OneNote.TaggingKit.common.ui/ScopeSelector/ScopeChanged.md) event |
| delegate [ScopeChangedEventEventHandler](./WetHatLab.OneNote.TaggingKit.common.ui/ScopeChangedEventEventHandler.md) | handlers for the [`ScopeChanged`](./WetHatLab.OneNote.TaggingKit.common.ui/ScopeSelector/ScopeChanged.md) event |
| class [ScopeSelector](./WetHatLab.OneNote.TaggingKit.common.ui/ScopeSelector.md) | Interaction logic for ScopeSelector.xaml |
| class [ScopeSelectorModel](./WetHatLab.OneNote.TaggingKit.common.ui/ScopeSelectorModel.md) | View model for the [`ScopeSelector`](./WetHatLab.OneNote.TaggingKit.common.ui/ScopeSelector.md) UI control. |
| class [SearchScopeFacade](./WetHatLab.OneNote.TaggingKit.common.ui/SearchScopeFacade.md) | Search Scope UI facade |
| class [SelectableTag](./WetHatLab.OneNote.TaggingKit.common.ui/SelectableTag.md) | A control to render tags which can be filtered. |
| class [SelectableTagModel](./WetHatLab.OneNote.TaggingKit.common.ui/SelectableTagModel.md) | View model for tags which change state when they are selected. |
| class [SelectedTagModel](./WetHatLab.OneNote.TaggingKit.common.ui/SelectedTagModel.md) | A view model which decorates instances of [`SelectableTagModel`](./WetHatLab.OneNote.TaggingKit.common.ui/SelectableTagModel.md) where the [`IsSelected`](./WetHatLab.OneNote.TaggingKit.common.ui/SelectableTagModel/IsSelected.md) is `true`. |
| class [Tag](./WetHatLab.OneNote.TaggingKit.common.ui/Tag.md) | Interaction logic for Tag.xaml |
| class [TagInputBox](./WetHatLab.OneNote.TaggingKit.common.ui/TagInputBox.md) | An input control for a comma separated list of tag names. |
| class [TagList](./WetHatLab.OneNote.TaggingKit.common.ui/TagList.md) | View for tags represented by data context objects coming from lists of type [`TagSource`](./WetHatLab.OneNote.TaggingKit.common.ui/TagList/TagSource.md). |
| class [TagModel](./WetHatLab.OneNote.TaggingKit.common.ui/TagModel.md) | A basic data context implementation for showing tags in list views. |
| class [TagModelKey](./WetHatLab.OneNote.TaggingKit.common.ui/TagModelKey.md) | Sortable key for OneNote page tags for use in view models. |
| class [TagSelectedEventArgs](./WetHatLab.OneNote.TaggingKit.common.ui/TagSelectedEventArgs.md) | Event details for tag selection events. |
| abstract class [WindowViewModelBase](./WetHatLab.OneNote.TaggingKit.common.ui/WindowViewModelBase.md) | Base class for view models supporting the MVVM pattern for top level add-in windows. |

## WetHatLab.OneNote.TaggingKit.edit namespace

| public type | description |
| --- | --- |
| class [TagEditor](./WetHatLab.OneNote.TaggingKit.edit/TagEditor.md) | The Tag Editor dialog. |
| class [TagEditorDesignerModel](./WetHatLab.OneNote.TaggingKit.edit/TagEditorDesignerModel.md) | View model implementation for the UI designer |
| class [TagEditorModel](./WetHatLab.OneNote.TaggingKit.edit/TagEditorModel.md) | View Model to support the tag editor dialog [`TagEditor`](./WetHatLab.OneNote.TaggingKit.edit/TagEditor.md). |
| enum [TaggingScope](./WetHatLab.OneNote.TaggingKit.edit/TaggingScope.md) | Classification of a range of OneNote pages. |
| class [TaggingScopeDescriptor](./WetHatLab.OneNote.TaggingKit.edit/TaggingScopeDescriptor.md) | Descriptor for a range of pages the tags will be applied to. |

## WetHatLab.OneNote.TaggingKit.find namespace

| public type | description |
| --- | --- |
| class [AnyRefinementTag](./WetHatLab.OneNote.TaggingKit.find/AnyRefinementTag.md) | A refinement tag which is a member of set of tags any of which is required for a page in order to pass the filter. |
| class [BlockedRefinementTag](./WetHatLab.OneNote.TaggingKit.find/BlockedRefinementTag.md) | A refinement tag required to not be on a OneNote page in order to pass the filter. |
| class [ExceptWithTagsFilter](./WetHatLab.OneNote.TaggingKit.find/ExceptWithTagsFilter.md) | A set-subtraction based tag filter. |
| class [FindTaggedPages](./WetHatLab.OneNote.TaggingKit.find/FindTaggedPages.md) | Interaction logic for TagSearch.xaml |
| class [FindTaggedPagesDesignerModel](./WetHatLab.OneNote.TaggingKit.find/FindTaggedPagesDesignerModel.md) | Designer support for the [`FindTaggedPages`](./WetHatLab.OneNote.TaggingKit.find/FindTaggedPages.md) window. |
| class [FindTaggedPagesModel](./WetHatLab.OneNote.TaggingKit.find/FindTaggedPagesModel.md) | View model backing the UI to find tagged pages. |
| class [HitHighlightedPageLink](./WetHatLab.OneNote.TaggingKit.find/HitHighlightedPageLink.md) | Control to render a hit highlighted link to a OneNote page. |
| class [HitHighlightedPageLinkDesignerModel](./WetHatLab.OneNote.TaggingKit.find/HitHighlightedPageLinkDesignerModel.md) | Design time view model for the [`HitHighlightedPageLink`](./WetHatLab.OneNote.TaggingKit.find/HitHighlightedPageLink.md) control. |
| class [HitHighlightedPageLinkKey](./WetHatLab.OneNote.TaggingKit.find/HitHighlightedPageLinkKey.md) | Sortable key to support the ranked display of [`HitHighlightedPageLink`](./WetHatLab.OneNote.TaggingKit.find/HitHighlightedPageLink.md) controls. |
| class [HitHighlightedPageLinkModel](./WetHatLab.OneNote.TaggingKit.find/HitHighlightedPageLinkModel.md) | View model to support the [`HitHighlightedPageLink`](./WetHatLab.OneNote.TaggingKit.find/HitHighlightedPageLink.md) control. |
| interface [IHitHighlightedPageLinkModel](./WetHatLab.OneNote.TaggingKit.find/IHitHighlightedPageLinkModel.md) | Contract for view models supporting the [`HitHighlightedPageLink`](./WetHatLab.OneNote.TaggingKit.find/HitHighlightedPageLink.md) control. |
| abstract class [RefinementTagBase](./WetHatLab.OneNote.TaggingKit.find/RefinementTagBase.md) | Aa abstract decorator base class for OneNote page tags used to filter sets of OneNote pages base on specific rules implemented by subclasses. |
| class [RefinementTagModel](./WetHatLab.OneNote.TaggingKit.find/RefinementTagModel.md) | View model to support [`SelectableTag`](./WetHatLab.OneNote.TaggingKit.common.ui/SelectableTag.md) controls. |
| class [RefinementTagModelSource](./WetHatLab.OneNote.TaggingKit.find/RefinementTagModelSource.md) | Observable collection of view models of type [`RefinementTagModel`](./WetHatLab.OneNote.TaggingKit.find/RefinementTagModel.md).. |
| class [RequiredRefinementTag](./WetHatLab.OneNote.TaggingKit.find/RequiredRefinementTag.md) | A refinement tag required to bo on a OneNote page in order to pass the filter.. |
| abstract class [TagFilterBase](./WetHatLab.OneNote.TaggingKit.find/TagFilterBase.md) | Abstract base class to define and apply rules to filter down OneNote pages based on tags. |
| class [TagFilterPanel](./WetHatLab.OneNote.TaggingKit.find/TagFilterPanel.md) | interaction logic for the user control. |
| class [TagFilterPanelDesignerModel](./WetHatLab.OneNote.TaggingKit.find/TagFilterPanelDesignerModel.md) |  |
| class [TagFilterPanelModel](./WetHatLab.OneNote.TaggingKit.find/TagFilterPanelModel.md) | View model backing the [`TagFilterPanel`](./WetHatLab.OneNote.TaggingKit.find/TagFilterPanel.md) control. |
| class [WithAllTagsFilter](./WetHatLab.OneNote.TaggingKit.find/WithAllTagsFilter.md) | A set-intersection based tag filter. |
| class [WithAnyTagsFilter](./WetHatLab.OneNote.TaggingKit.find/WithAnyTagsFilter.md) | A set-union based tag filter. |

## WetHatLab.OneNote.TaggingKit.HierarchyBuilder namespace

| public type | description |
| --- | --- |
| class [HierarchyNode](./WetHatLab.OneNote.TaggingKit.HierarchyBuilder/HierarchyNode.md) | Representation of an element in the hierarchy of the OneNote page tree. |
| class [PageHierarchy](./WetHatLab.OneNote.TaggingKit.HierarchyBuilder/PageHierarchy.md) | Proxy representation of a OneNote page hierarchy. |
| class [PageNode](./WetHatLab.OneNote.TaggingKit.HierarchyBuilder/PageNode.md) | Representation of a OneNote page with its page level tags. |

## WetHatLab.OneNote.TaggingKit.manage namespace

| public type | description |
| --- | --- |
| class [RemovableTag](./WetHatLab.OneNote.TaggingKit.manage/RemovableTag.md) | Interaction logic for RemovableTag.xaml |
| class [RemovableTagModel](./WetHatLab.OneNote.TaggingKit.manage/RemovableTagModel.md) | View model backing a [`RemovableTag`](./WetHatLab.OneNote.TaggingKit.manage/RemovableTag.md) user control. |
| class [TagManager](./WetHatLab.OneNote.TaggingKit.manage/TagManager.md) | Interaction logic for TagManager.xaml user control |
| class [TagManagerDesignerModel](./WetHatLab.OneNote.TaggingKit.manage/TagManagerDesignerModel.md) | View model to support the design mode for the [`TagManager`](./WetHatLab.OneNote.TaggingKit.manage/TagManager.md) dialog |
| class [TagManagerModel](./WetHatLab.OneNote.TaggingKit.manage/TagManagerModel.md) | View model backing the [`TagManager`](./WetHatLab.OneNote.TaggingKit.manage/TagManager.md) dialog. |

## WetHatLab.OneNote.TaggingKit.PageBuilder namespace

| public type | description |
| --- | --- |
| class [Cell](./WetHatLab.OneNote.TaggingKit.PageBuilder/Cell.md) | Proxy class for table cells |
| class [CellCollection](./WetHatLab.OneNote.TaggingKit.PageBuilder/CellCollection.md) | Collection of table cell proxies defining a table row. |
| class [DefinitionObjectBase](./WetHatLab.OneNote.TaggingKit.PageBuilder/DefinitionObjectBase.md) | Base class for indexed definition proxy objects. |
| abstract class [DefinitionObjectCollection&lt;T&gt;](./WetHatLab.OneNote.TaggingKit.PageBuilder/DefinitionObjectCollection-1.md) | An abstract base class for collections of definition elements on a OneNote page . |
| class [Meta](./WetHatLab.OneNote.TaggingKit.PageBuilder/Meta.md) | Proxy for a `one:Meta`meta element on a OneNote page document. |
| class [MetaCollection](./WetHatLab.OneNote.TaggingKit.PageBuilder/MetaCollection.md) | The collection of Meta objects for Meta XML elements on a OneNote page. Meta objects with special semantics are accessible via the properties: |
| class [NamedObjectBase](./WetHatLab.OneNote.TaggingKit.PageBuilder/NamedObjectBase.md) | Base class for proxy objeccts containing a XML element with a `name` attribute. |
| class [OE](./WetHatLab.OneNote.TaggingKit.PageBuilder/OE.md) | Proxy object for OneNote page content elements. |
| class [OEChildren](./WetHatLab.OneNote.TaggingKit.PageBuilder/OEChildren.md) | Proxy object for a indented group of OneNote content elements. |
| class [OESavedSearch](./WetHatLab.OneNote.TaggingKit.PageBuilder/OESavedSearch.md) | Proxy class for a OneNpte element structure representing a Saved Search. |
| class [OESavedSearchCollection](./WetHatLab.OneNote.TaggingKit.PageBuilder/OESavedSearchCollection.md) | Collection of saved searches on a OneNote page. |
| class [OET](./WetHatLab.OneNote.TaggingKit.PageBuilder/OET.md) | Proxy class for OneNote elements with embedded text content. |
| class [OETable](./WetHatLab.OneNote.TaggingKit.PageBuilder/OETable.md) | Proxy class for a OneNote elements with and embedded table. |
| class [OETaglist](./WetHatLab.OneNote.TaggingKit.PageBuilder/OETaglist.md) | A OneNote paragraph proxy containing a comma separated list of tags. |
| class [OneNotePage](./WetHatLab.OneNote.TaggingKit.PageBuilder/OneNotePage.md) | Local proxy of a OneNote page. |
| class [PageObjectBase](./WetHatLab.OneNote.TaggingKit.PageBuilder/PageObjectBase.md) | Base class for OneNote page element proxy objects. |
| abstract class [PageObjectCollectionBase&lt;Towner,Titem&gt;](./WetHatLab.OneNote.TaggingKit.PageBuilder/PageObjectCollectionBase-2.md) | A collection of proxy objects for XML elements of the same type, |
| enum [PageSchemaPosition](./WetHatLab.OneNote.TaggingKit.PageBuilder/PageSchemaPosition.md) |  |
| class [PageSettings](./WetHatLab.OneNote.TaggingKit.PageBuilder/PageSettings.md) | Proxy object for `&lt;one:PageSettings&gt;` elements on a OneNote page document. |
| class [PageStructureObjectBase](./WetHatLab.OneNote.TaggingKit.PageBuilder/PageStructureObjectBase.md) |  |
| abstract class [PageStructureObjectCollection&lt;T&gt;](./WetHatLab.OneNote.TaggingKit.PageBuilder/PageStructureObjectCollection-1.md) | Abstract base class for collections of new or existing OneNote top-level structure object proxies which have to appear in a given sequence on the OneNote page. |
| class [QuickStyleDef](./WetHatLab.OneNote.TaggingKit.PageBuilder/QuickStyleDef.md) | Proxy object class for `one:QuickStyleDef` style definition elements on a OneNote page document. |
| class [QuickStyleDefCollection](./WetHatLab.OneNote.TaggingKit.PageBuilder/QuickStyleDefCollection.md) | Collection of style definition proxy objects. |
| class [Row](./WetHatLab.OneNote.TaggingKit.PageBuilder/Row.md) | Proxy class for OneNote table rows. |
| class [RowCollection](./WetHatLab.OneNote.TaggingKit.PageBuilder/RowCollection.md) | A Collection of table row proxy elements. |
| class [Table](./WetHatLab.OneNote.TaggingKit.PageBuilder/Table.md) | Proxy class for OneNote tables. |
| class [Tag](./WetHatLab.OneNote.TaggingKit.PageBuilder/Tag.md) | Proxy object for OneNote tag `one:Tag` elements. |
| class [TagCollection](./WetHatLab.OneNote.TaggingKit.PageBuilder/TagCollection.md) | The collection of tags below an `one:OE` content XML element |
| class [TagDef](./WetHatLab.OneNote.TaggingKit.PageBuilder/TagDef.md) |  |
| class [TagDefCollection](./WetHatLab.OneNote.TaggingKit.PageBuilder/TagDefCollection.md) | A collection of tag definitions. |
| enum [TagDisplay](./WetHatLab.OneNote.TaggingKit.PageBuilder/TagDisplay.md) | Enumeration of ways to display tags on a Onenote Page |
| enum [TagProcessClassification](./WetHatLab.OneNote.TaggingKit.PageBuilder/TagProcessClassification.md) | The enumeration of process types a tag can participate in. |
| class [Title](./WetHatLab.OneNote.TaggingKit.PageBuilder/Title.md) | Proxy object for `&lt;one:Title&gt;` elements on a OneNote page document. |

## WetHatLab.OneNote.TaggingKit.Tagger namespace

| public type | description |
| --- | --- |
| class [BackgroundTagger](./WetHatLab.OneNote.TaggingKit.Tagger/BackgroundTagger.md) | Background OneNote page tagger. |
| class [TaggingJob](./WetHatLab.OneNote.TaggingKit.Tagger/TaggingJob.md) | A tagging job to be performed in the background. |
| enum [TagOperation](./WetHatLab.OneNote.TaggingKit.Tagger/TagOperation.md) | Tagging Operations |

## XamlGeneratedNamespace namespace

| public type | description |
| --- | --- |
| class [GeneratedInternalTypeHelper](./XamlGeneratedNamespace/GeneratedInternalTypeHelper.md) | GeneratedInternalTypeHelper |

<!-- DO NOT EDIT: generated by xmldocmd for OneNoteTaggingKit.dll -->
