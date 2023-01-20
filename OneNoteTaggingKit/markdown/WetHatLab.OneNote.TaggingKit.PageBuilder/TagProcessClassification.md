# TagProcessClassification enumeration

The enumeration of process types a tag can participate in.

```csharp
public enum TagProcessClassification
```

## Values

| name | value | description |
| --- | --- | --- |
| InTitleMarker | `0` | A OneNote tag in the page title whose name is the list of page tags. |
| BelowTitleMarker | `1` | A OneNote tag marking a `one:OE` element containing a list of page tags. |
| SavedSearchMarker | `2` | A OneNote tag marking a `one:OE` element containing a saved search. |
| OneNoteTag | `3` | A regular, importable OneNote Paragrapg tag. |
| PageTag | `4` | A regular page tag managed by the Tagging Kit. |

## See Also

* namespace [WetHatLab.OneNote.TaggingKit.PageBuilder](../OneNoteTaggingKit.md)

<!-- DO NOT EDIT: generated by xmldocmd for OneNoteTaggingKit.dll -->