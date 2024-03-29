# BackgroundTagger class

Background OneNote page tagger.

```csharp
public sealed class BackgroundTagger : IDisposable
```

## Public Members

| name | description |
| --- | --- |
| [BackgroundTagger](BackgroundTagger/BackgroundTagger.md)(…) | Create a new instance of a background page tagger. |
| [JobCount](BackgroundTagger/JobCount.md) { get; } | Get the number of jobs executed by this the tagger. |
| [LastJobType](BackgroundTagger/LastJobType.md) { get; } | Get the type of last executed job. |
| [PendingJobCount](BackgroundTagger/PendingJobCount.md) { get; } | Get the number of pending background jobs. |
| [Add](BackgroundTagger/Add.md)(…) | Schedule a tagging job for background operation. |
| [Dispose](BackgroundTagger/Dispose.md)() | Dispose the background tagger. |
| [Run](BackgroundTagger/Run.md)() | Run the background tagger. |

## See Also

* namespace [WetHatLab.OneNote.TaggingKit.Tagger](../OneNoteTaggingKit.md)

<!-- DO NOT EDIT: generated by xmldocmd for OneNoteTaggingKit.dll -->
