# MetaCollection class

The collection of Meta objects for Meta XML elements on a OneNote page. Meta objects with special semantics are accessible via the properties:

* [`PageTags`](./MetaCollection/PageTags.md)
* [`SearchScope`](./MetaCollection/SearchScope.md)/&gt;

```csharp
public class MetaCollection : PageStructureObjectCollection<Meta>
```

## Public Members

| name | description |
| --- | --- |
| [MetaCollection](MetaCollection/MetaCollection.md)(…) | Intitialize the collection Meta element proxies from an OneNote page. |
| [PageTags](MetaCollection/PageTags.md) { get; set; } | Get the comma separated list of page tags on this page. |
| [SearchScope](MetaCollection/SearchScope.md) { get; set; } | Get the ID of the hierarchy element the saved search on this page was created for. |
| const [PageTagsMetaKey](MetaCollection/PageTagsMetaKey.md) | The Meta key of page tags. |
| const [SearchScopeMetaKey](MetaCollection/SearchScopeMetaKey.md) | The Meta key for scopes of saved searches. |

## Protected Members

| name | description |
| --- | --- |
| override [CreateElementProxy](MetaCollection/CreateElementProxy.md)(…) | Factory method to Create a meta element for a Meta element which exists on a OneNote page. |

## See Also

* class [PageStructureObjectCollection&lt;T&gt;](./PageStructureObjectCollection-1.md)
* class [Meta](./Meta.md)
* namespace [WetHatLab.OneNote.TaggingKit.PageBuilder](../OneNoteTaggingKit.md)

<!-- DO NOT EDIT: generated by xmldocmd for OneNoteTaggingKit.dll -->