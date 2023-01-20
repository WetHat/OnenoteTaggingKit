# CellCollection class

Collection of table cell proxies defining a table row.

```csharp
public class CellCollection : PageObjectCollectionBase<Row, Cell>
```

## Public Members

| name | description |
| --- | --- |
| [CellCollection](CellCollection/CellCollection.md)(…) | Initialize the proxy object collection of table cell elements contained under a table row element in a table on a OneNote page XML document. |
| [Count](CellCollection/Count.md) { get; } | Get the number of cells in this colelction. |
| [Item](CellCollection/Item.md) { get; } | Get a cell proxy from this collection. |
| [AddCell](CellCollection/AddCell.md)(…) | Add a cell to this row. |

## Protected Members

| name | description |
| --- | --- |
| override [Add](CellCollection/Add.md)(…) | Add a cell to the collection of cells in a table row. |
| override [CreateElementProxy](CellCollection/CreateElementProxy.md)(…) | Create a new proxy object for a cell in a OneNote tyble. |

## See Also

* class [PageObjectCollectionBase&lt;Towner,Titem&gt;](./PageObjectCollectionBase-2.md)
* class [Row](./Row.md)
* class [Cell](./Cell.md)
* namespace [WetHatLab.OneNote.TaggingKit.PageBuilder](../OneNoteTaggingKit.md)

<!-- DO NOT EDIT: generated by xmldocmd for OneNoteTaggingKit.dll -->