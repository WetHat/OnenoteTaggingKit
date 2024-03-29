# PageTagSet.Parse method (1 of 2)

Parse a collection of tag names into [`PageTag`](../PageTag.md) instances.

```csharp
public static IEnumerable<PageTag> Parse(IEnumerable<string> tagnames, TagFormat format)
```

| parameter | description |
| --- | --- |
| tagnames | Collection of plain text tag names. No HTML markup allowed. |
| format | The tag formatting to apply. |

## See Also

* class [PageTag](../PageTag.md)
* enum [TagFormat](../TagFormat.md)
* class [PageTagSet](../PageTagSet.md)
* namespace [WetHatLab.OneNote.TaggingKit.common](../../OneNoteTaggingKit.md)

---

# PageTagSet.Parse method (2 of 2)

Parse a collection of tag names into [`PageTag`](../PageTag.md) instances.

```csharp
public static IEnumerable<PageTag> Parse(string taglist, TagFormat format)
```

| parameter | description |
| --- | --- |
| taglist | Comma separated list of plain text tag names. No HTML markup allowed. |
| format | The tag formatting to apply. |

## See Also

* class [PageTag](../PageTag.md)
* enum [TagFormat](../TagFormat.md)
* class [PageTagSet](../PageTagSet.md)
* namespace [WetHatLab.OneNote.TaggingKit.common](../../OneNoteTaggingKit.md)

<!-- DO NOT EDIT: generated by xmldocmd for OneNoteTaggingKit.dll -->
