# TagModel class

A basic data context implementation for showing tags in list views.

```csharp
public class TagModel : ObservableObject, ISortableKeyedItem<string, string>, ITagModel
```

## Public Members

| name | description |
| --- | --- |
| [TagModel](TagModel/TagModel.md)() | Create a new view model for tags. |
| [Key](TagModel/Key.md) { get; } | Get the unique key of this tag. |
| [SortKey](TagModel/SortKey.md) { get; } | Get a key for this tag which is suitable for sorting the tag models- |
| virtual [Tag](TagModel/Tag.md) { get; set; } | Get or set the page tag represented by this model. |
| [TagIndicator](TagModel/TagIndicator.md) { get; set; } | Get/set the tag indicator (postfix string). |
| [TagIndicatorColor](TagModel/TagIndicatorColor.md) { get; set; } | Get/set the forground color of the tag indicator |
| [TagName](TagModel/TagName.md) { get; } | Get or set the name of a page tag represented by this model. |
| [TagTypePostfix](TagModel/TagTypePostfix.md) { get; protected set; } | Get the tag postfix marker. |
| [TagTypePrefix](TagModel/TagTypePrefix.md) { get; protected set; } | Get the tag prefix marker. |
| [TagVisibility](TagModel/TagVisibility.md) { get; protected set; } | The visibility of the tag. |
| override [Equals](TagModel/Equals.md)(…) | Determine if this instance is equal to another object. |
| override [GetHashCode](TagModel/GetHashCode.md)() | Get the hash code for this model,. |

## Protected Members

| name | description |
| --- | --- |
| virtual [TagModelPropertyChanged](TagModel/TagModelPropertyChanged.md)(…) | Property change handler listening to changes to instances of this class. |

## Remarks

Can be used as-is for simple tag representations or can be subclassed to add additional functionality such as highlighting.

## See Also

* class [ObservableObject](../WetHatLab.OneNote.TaggingKit.common/ObservableObject.md)
* interface [ISortableKeyedItem&lt;TSort,TKey&gt;](../WetHatLab.OneNote.TaggingKit.common/ISortableKeyedItem-2.md)
* interface [ITagModel](./ITagModel.md)
* namespace [WetHatLab.OneNote.TaggingKit.common.ui](../OneNoteTaggingKit.md)

<!-- DO NOT EDIT: generated by xmldocmd for OneNoteTaggingKit.dll -->
