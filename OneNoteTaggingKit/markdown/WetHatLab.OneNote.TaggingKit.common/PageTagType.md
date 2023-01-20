# PageTagType enumeration

Enumeration of types of Tagging Kit recognized page tags which appear on OneNote pages.

```csharp
public enum PageTagType
```

## Values

| name | value | description |
| --- | --- | --- |
| PlainTag | `0` | A regular page tag without any annotation. |
| HashTag | `1` | A hashtag. |
| ImportedOneNoteTag | `2` | An importated OneNote tag. |
| ImportedHashTag | `3` | An imported hashtag. |
| Unknown | `4` | Unknown tag type. |

## Remarks

This class implements the required interfaces to make it suitable for HashSets.

## See Also

* namespace [WetHatLab.OneNote.TaggingKit.common](../OneNoteTaggingKit.md)

<!-- DO NOT EDIT: generated by xmldocmd for OneNoteTaggingKit.dll -->