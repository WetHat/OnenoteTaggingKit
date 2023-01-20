# TagInputEventArgs constructor

Create a new instance of the event metadata object.

```csharp
internal TagInputEventArgs(RoutedEvent routedEvent, object source, IEnumerable<string> tags, 
    KeyEventArgs e)
```

| parameter | description |
| --- | --- |
| routedEvent | Routed event which fired |
| source | Control which fired the event |
| tags | The page tags the event was fired for. |
| e | the keyboard event arguments. Can be null if the event was generated without keyboard input. |

## See Also

* class [TagInputEventArgs](../TagInputEventArgs.md)
* namespace [WetHatLab.OneNote.TaggingKit.common](../../OneNoteTaggingKit.md)

<!-- DO NOT EDIT: generated by xmldocmd for OneNoteTaggingKit.dll -->