# ObservableDictionary&lt;TKey,TValue&gt; class

A dictionary which notifies subscribed listeners about content changes.

```csharp
public class ObservableDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    where TKey : IEquatable<TKey>
    where TValue : IKeyedItem<TKey>
```

| parameter | description |
| --- | --- |
| TKey | type of the key used in the dictionary |
| TValue | type of objects stored in the dictionary |

## Public Members

| name | description |
| --- | --- |
| [ObservableDictionary](ObservableDictionary-2/ObservableDictionary.md)() | The default constructor. |
| [Count](ObservableDictionary-2/Count.md) { get; } | Get the number of items in the dictionary. |
| [IsReadOnly](ObservableDictionary-2/IsReadOnly.md) { get; } | Determine if this instance of the dictionary is mutable. |
| [Item](ObservableDictionary-2/Item.md) { get; set; } | Get or add an item to the dictionary |
| [Keys](ObservableDictionary-2/Keys.md) { get; } | Get the collection of keys of the items in the dictionary |
| [Values](ObservableDictionary-2/Values.md) { get; } | Get the collection of items contained in the dictionary. |
| event [CollectionChanged](ObservableDictionary-2/CollectionChanged.md) | Event to inform subscribers about changes to the dictionary. |
| [Add](ObservableDictionary-2/Add.md)(…) | Add an item to the dictionary, if not already present. (2 methods) |
| [Clear](ObservableDictionary-2/Clear.md)() | Remove all items from the dictionary. |
| [Contains](ObservableDictionary-2/Contains.md)(…) | Determine if the dictionary contains a prticular entry |
| [ContainsKey](ObservableDictionary-2/ContainsKey.md)(…) | Check of the dictionary contains an item with a given key |
| [CopyTo](ObservableDictionary-2/CopyTo.md)(…) | Copy all items in the dictionary to an array |
| [ExceptWith](ObservableDictionary-2/ExceptWith.md)(…) | Perform a set subtract. The given items are removed from the set of items in the dictionary. |
| [GetEnumerator](ObservableDictionary-2/GetEnumerator.md)() | get an enumerator for the entries in the dictionary. |
| [IntersectWith](ObservableDictionary-2/IntersectWith.md)(…) | Perform a set intersect. Only items present in both the dictionary and the argument remain in the dictionary. |
| [Remove](ObservableDictionary-2/Remove.md)(…) | Remove an item with a given key. (2 methods) |
| [Reset](ObservableDictionary-2/Reset.md)(…) | Reset the dictionary with new content. (2 methods) |
| [TryGetValue](ObservableDictionary-2/TryGetValue.md)(…) | Try to find an item in the dictionary. |
| [UnionWith](ObservableDictionary-2/UnionWith.md)(…) | Perform set union of the given items with the items already in the dictionary |

## Remarks

This class is not thread save.

## See Also

* interface [IKeyedItem&lt;T&gt;](./IKeyedItem-1.md)
* namespace [WetHatLab.OneNote.TaggingKit.common](../OneNoteTaggingKit.md)

<!-- DO NOT EDIT: generated by xmldocmd for OneNoteTaggingKit.dll -->
