# ObservableDictionary(*TKey*, *TValue*).CopyTo Method 
 _**\[This is preliminary documentation and is subject to change.\]**_

Copy all items in the dictionary to an array

**Namespace:**&nbsp;<a href="bcdbab9c-63d1-48a4-6937-af53fb8d9a55.md">WetHatLab.OneNote.TaggingKit.common</a><br />**Assembly:**&nbsp;OneNoteTaggingKit (in OneNoteTaggingKit.dll) Version: 3.8.7617.35763

## Syntax

**C#**<br />
``` C#
public void CopyTo(
	KeyValuePair<TKey, TValue>[] array,
	int arrayIndex
)
```


#### Parameters
&nbsp;<dl><dt>array</dt><dd>Type: <a href="http://msdn2.microsoft.com/en-us/library/5tbh8a42" target="_blank">System.Collections.Generic.KeyValuePair</a>(<a href="b95e4b9e-1bee-ddc0-1db7-61a35069e23a.md">*TKey*</a>, <a href="b95e4b9e-1bee-ddc0-1db7-61a35069e23a.md">*TValue*</a>)[]<br />array to copy the items from the dictioary into</dd><dt>arrayIndex</dt><dd>Type: <a href="http://msdn2.microsoft.com/en-us/library/td2s409d" target="_blank">System.Int32</a><br />start index in the array for the copy</dd></dl>

#### Implements
<a href="http://msdn2.microsoft.com/en-us/library/0efx51xw" target="_blank">ICollection(T).CopyTo(T[], Int32)</a><br />

## See Also


#### Reference
<a href="b95e4b9e-1bee-ddc0-1db7-61a35069e23a.md">ObservableDictionary(TKey, TValue) Class</a><br /><a href="bcdbab9c-63d1-48a4-6937-af53fb8d9a55.md">WetHatLab.OneNote.TaggingKit.common Namespace</a><br />