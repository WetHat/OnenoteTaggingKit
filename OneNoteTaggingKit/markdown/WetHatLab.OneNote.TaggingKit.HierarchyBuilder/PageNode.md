# PageNode class

Representation of a OneNote page with its page level tags.

```csharp
public class PageNode : HierarchyNode
```

## Public Members

| name | description |
| --- | --- |
| [IsInRecycleBin](PageNode/IsInRecycleBin.md) { get; } | Determine if the tagged pages is recycled |
| [IsSelected](PageNode/IsSelected.md) { get; } | Get the selection status of the page |
| [Key](PageNode/Key.md) { get; } | Get pages unique key suitable for hashing |
| [Tags](PageNode/Tags.md) { get; } | Get the collection of tags on this page |
| override [Equals](PageNode/Equals.md)(…) | Check two page objects for equality |
| override [GetHashCode](PageNode/GetHashCode.md)() | Compute the hashcode |

## Internal Members

| name | description |
| --- | --- |
| [PageNode](PageNode/PageNode.md)(…) | Create an internal representation of a page returned from FindMeta |

## See Also

* class [HierarchyNode](./HierarchyNode.md)
* namespace [WetHatLab.OneNote.TaggingKit.HierarchyBuilder](../OneNoteTaggingKit.md)

<!-- DO NOT EDIT: generated by xmldocmd for OneNoteTaggingKit.dll -->