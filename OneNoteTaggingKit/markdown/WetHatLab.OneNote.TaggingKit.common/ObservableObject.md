# ObservableObject class

Base class for objects which expose observable properties.

```csharp
public class ObservableObject : IDisposable, INotifyPropertyChanged
```

## Public Members

| name | description |
| --- | --- |
| [ObservableObject](ObservableObject/ObservableObject.md)() | The default constructor. |
| event [PropertyChanged](ObservableObject/PropertyChanged.md) | Event to notify registered handlers about property changes |
| virtual [Dispose](ObservableObject/Dispose.md)() | Clear all property handlers for this object. |

## Protected Members

| name | description |
| --- | --- |
| [RaisePropertyChanged](ObservableObject/RaisePropertyChanged.md)(…) | Raise a change event for |

## Remarks

To support working with view models in background tasks the use of dependency objects is not recommended because of their thread affinity. Instead observable properties should raise PropertyChanged events. Methods to do that in a type save way are provided in this base class.

## See Also

* namespace [WetHatLab.OneNote.TaggingKit.common](../OneNoteTaggingKit.md)

<!-- DO NOT EDIT: generated by xmldocmd for OneNoteTaggingKit.dll -->
