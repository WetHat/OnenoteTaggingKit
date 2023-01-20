# FilterableTagModel.Highlighter property

Set/get the highlighter object which is used to mark portions of the tag name which match one or more strings.

```csharp
public TextSplitter Highlighter { get; set; }
```

## Remarks

Setting this property has a side effect on the property [`HighlightedTagName`](./HighlightedTagName.md). Tags which do not match the highlighting pattern are collapsed.

## See Also

* class [TextSplitter](../../WetHatLab.OneNote.TaggingKit.common/TextSplitter.md)
* class [FilterableTagModel](../FilterableTagModel.md)
* namespace [WetHatLab.OneNote.TaggingKit.common.ui](../../OneNoteTaggingKit.md)

<!-- DO NOT EDIT: generated by xmldocmd for OneNoteTaggingKit.dll -->