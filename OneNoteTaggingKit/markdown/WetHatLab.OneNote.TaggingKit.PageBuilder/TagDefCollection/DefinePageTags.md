# TagDefCollection.DefinePageTags method

Define the tags which should be on the page.

```csharp
public void DefinePageTags(PageTagSet tags)
```

| parameter | description |
| --- | --- |
| tags | List of page tags on a OneNote page. |

## Remarks

We recycle the tag definitions sequencially knowing that setting the tag also changes the definition name accordingly. If we have more definitions than tags, the extra definitions are deleted.

## See Also

* class [PageTagSet](../../WetHatLab.OneNote.TaggingKit.common/PageTagSet.md)
* class [TagDefCollection](../TagDefCollection.md)
* namespace [WetHatLab.OneNote.TaggingKit.PageBuilder](../../OneNoteTaggingKit.md)

<!-- DO NOT EDIT: generated by xmldocmd for OneNoteTaggingKit.dll -->
