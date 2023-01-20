# OneNoteProxy.GetHierarchy method

Get the XML descriptor of nodes in the OneNote hierarchy

```csharp
public XDocument GetHierarchy(string nodeID, HierarchyScope scope)
```

| parameter | description |
| --- | --- |
| nodeID | id of the starting node |
| scope | scope of the nodes to return |

## Return Value

XML document describing the nodes in the OneNote hierarchy tree

## Exceptions

| exception | condition |
| --- | --- |
| COMException | Call to OneNote failed |

## Remarks

Only basic information (as of OneNote 2010) is returned.

## See Also

* class [OneNoteProxy](../OneNoteProxy.md)
* namespace [WetHatLab.OneNote.TaggingKit](../../OneNoteTaggingKit.md)

<!-- DO NOT EDIT: generated by xmldocmd for OneNoteTaggingKit.dll -->