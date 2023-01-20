# TagsAndPages.LoadPageTags method

Load tags the pages from a subtree of the OneNote page hierarchy.

```csharp
internal void LoadPageTags(TagContext context, bool omitUntaggedPages = true)
```

| parameter | description |
| --- | --- |
| context | The context from where to get pages. |
| omitUntaggedPages | Optional flag to exclude untagged pages when `true` or include empty pages when `false`. Default is `true`. |

## See Also

* class [TagsAndPages](../TagsAndPages.md)
* namespace [WetHatLab.OneNote.TaggingKit.common](../../OneNoteTaggingKit.md)

<!-- DO NOT EDIT: generated by xmldocmd for OneNoteTaggingKit.dll -->