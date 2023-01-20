# TextSplitter class

Split text at pattern matches.

```csharp
public class TextSplitter
```

## Internal Members

| name | description |
| --- | --- |
| [TextSplitter](TextSplitter/TextSplitter.md)(…) | Create a new text splitter instance (2 constructors) |
| [SplitPattern](TextSplitter/SplitPattern.md) { get; } | Get the regular expression used for splitting. |
| [SplitText](TextSplitter/SplitText.md)(…) | Create a sequence of text fragments describing matching and non-matching fragments |

## Remarks

The text is split into [`TextFragment`](./TextFragment.md) sequences including all text fragments whether they match the pattern or not. See [`SplitText`](./TextSplitter/SplitText.md) for more details

## See Also

* namespace [WetHatLab.OneNote.TaggingKit.common](../OneNoteTaggingKit.md)

<!-- DO NOT EDIT: generated by xmldocmd for OneNoteTaggingKit.dll -->