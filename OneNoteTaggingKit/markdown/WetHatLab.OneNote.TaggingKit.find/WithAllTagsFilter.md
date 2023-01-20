# WithAllTagsFilter class

A set-intersection based tag filter.

```csharp
public class WithAllTagsFilter : TagFilterBase
```

## Public Members

| name | description |
| --- | --- |
| [WithAllTagsFilter](WithAllTagsFilter/WithAllTagsFilter.md)(…) | Initialize a page filter which requires pages to have all selected tags. |
| override [MakeRefinementTag](WithAllTagsFilter/MakeRefinementTag.md)(…) | Make a refinement tag which is required to be on a page to satisfy the filter requirement |

## Protected Members

| name | description |
| --- | --- |
| override [UpdateTagFilter](WithAllTagsFilter/UpdateTagFilter.md)(…) | Process changes to the collection of filter tags and update the collection of filtered pages accordingly. |

## Remarks

Computes the set of pages which have a given set tags.

## See Also

* class [TagFilterBase](./TagFilterBase.md)
* namespace [WetHatLab.OneNote.TaggingKit.find](../OneNoteTaggingKit.md)

<!-- DO NOT EDIT: generated by xmldocmd for OneNoteTaggingKit.dll -->