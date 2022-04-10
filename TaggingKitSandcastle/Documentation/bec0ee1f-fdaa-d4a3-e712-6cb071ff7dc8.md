# ObservableSortedList(*TSort*, *TKey*, *TValue*).AddAll Method 
 _**\[This is preliminary documentation and is subject to change.\]**_

Add items to the sorted collection in batches.

**Namespace:**&nbsp;<a href="bcdbab9c-63d1-48a4-6937-af53fb8d9a55.md">WetHatLab.OneNote.TaggingKit.common</a><br />**Assembly:**&nbsp;OneNoteTaggingKit (in OneNoteTaggingKit.dll) Version: 4.0.8135.22136

## Syntax

**C#**<br />
``` C#
internal void AddAll(
	IEnumerable<TValue> items
)
```


#### Parameters
&nbsp;<dl><dt>items</dt><dd>Type: System.Collections.Generic.IEnumerable(<a href="89870249-f56d-ac32-0b8d-d26e5712ecac.md">*TValue*</a>)<br />items to add</dd></dl>

## Remarks
Groups the given items into contiguous ranges of batches and adds each batch at once, firing one change notification per batch.

## See Also


#### Reference
<a href="89870249-f56d-ac32-0b8d-d26e5712ecac.md">ObservableSortedList(TSort, TKey, TValue) Class</a><br /><a href="bcdbab9c-63d1-48a4-6937-af53fb8d9a55.md">WetHatLab.OneNote.TaggingKit.common Namespace</a><br />