# TextFragment structure

Representation of a fragment of text which does or does not match a pattern

```csharp
public struct TextFragment
```

## Internal Members

| name | description |
| --- | --- |
| [TextFragment](TextFragment/TextFragment.md)(…) | create a new instance of a text fragment |
| [IsMatch](TextFragment/IsMatch.md) { get; } | Determine if this fragment is a match to a pattern |
| [Text](TextFragment/Text.md) { get; } | Get the text fragment |

## Remarks

instances of this classed are used by [`TextSplitter`](./TextSplitter.md) objects to split text according to matching patterns

## See Also

* class [TextSplitter](./TextSplitter.md)
* namespace [WetHatLab.OneNote.TaggingKit.common](../OneNoteTaggingKit.md)

<!-- DO NOT EDIT: generated by xmldocmd for OneNoteTaggingKit.dll -->
