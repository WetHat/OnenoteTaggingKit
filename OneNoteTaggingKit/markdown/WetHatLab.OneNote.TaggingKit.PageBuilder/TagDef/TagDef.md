# TagDef constructor (1 of 3)

Initialize a tag definition proxy object with a TagDef XML element from an OneNote page.

```csharp
public TagDef(OneNotePage page, XElement element)
```

| parameter | description |
| --- | --- |
| page | Proxy of the page which owns this object. |
| element | TagDef element from an OneNote page |

## See Also

* class [OneNotePage](../OneNotePage.md)
* class [TagDef](../TagDef.md)
* namespace [WetHatLab.OneNote.TaggingKit.PageBuilder](../../OneNoteTaggingKit.md)

---

# TagDef constructor (2 of 3)

Initialize a new instance of a tag definition proxy object.

```csharp
public TagDef(OneNotePage page, PageTag tag, int index)
```

| parameter | description |
| --- | --- |
| page | Proxy of the page which owns this object. |
| tag | The page tag for this definition |
| index | Definition index. |

## See Also

* class [OneNotePage](../OneNotePage.md)
* class [PageTag](../../WetHatLab.OneNote.TaggingKit.common/PageTag.md)
* class [TagDef](../TagDef.md)
* namespace [WetHatLab.OneNote.TaggingKit.PageBuilder](../../OneNoteTaggingKit.md)

---

# TagDef constructor (3 of 3)

Intitialize a new instance of a process tag definition

```csharp
public TagDef(OneNotePage page, string basename, int index, TagProcessClassification classification)
```

| parameter | description |
| --- | --- |
| page | The OneNote page proxy for which to define a process tag. |
| basename | The tag name without any suffixes. |
| index | THe definition index |
| classification | The tag process classification. |

## See Also

* class [OneNotePage](../OneNotePage.md)
* enum [TagProcessClassification](../TagProcessClassification.md)
* class [TagDef](../TagDef.md)
* namespace [WetHatLab.OneNote.TaggingKit.PageBuilder](../../OneNoteTaggingKit.md)

<!-- DO NOT EDIT: generated by xmldocmd for OneNoteTaggingKit.dll -->
