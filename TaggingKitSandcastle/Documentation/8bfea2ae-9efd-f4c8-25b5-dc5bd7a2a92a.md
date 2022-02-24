# NotifyDictionaryChangedEventArgs(*TKey*, *TValue*) Class
 _**\[This is preliminary documentation and is subject to change.\]**_

Event details describing to details of a changes to instances of <a href="b95e4b9e-1bee-ddc0-1db7-61a35069e23a">ObservableDictionary(TKey, TValue)</a>


## Inheritance Hierarchy
System.Object<br />&nbsp;&nbsp;System.EventArgs<br />&nbsp;&nbsp;&nbsp;&nbsp;WetHatLab.OneNote.TaggingKit.common.NotifyDictionaryChangedEventArgs(TKey, TValue)<br />
**Namespace:**&nbsp;<a href="bcdbab9c-63d1-48a4-6937-af53fb8d9a55">WetHatLab.OneNote.TaggingKit.common</a><br />**Assembly:**&nbsp;OneNoteTaggingKit (in OneNoteTaggingKit.dll) Version: 4.0.8059.31599

## Syntax

**C#**<br />
``` C#
public class NotifyDictionaryChangedEventArgs<TKey, TValue> : EventArgs
where TKey : Object, IEquatable<TKey>
where TValue : Object, IKeyedItem<TKey>

```


#### Type Parameters
&nbsp;<dl><dt>TKey</dt><dd>dictionary key type</dd><dt>TValue</dt><dd>dictionary value type</dd></dl>&nbsp;
The NotifyDictionaryChangedEventArgs(TKey, TValue) type exposes the following members.


## Constructors
&nbsp;<table><tr><th></th><th>Name</th><th>Description</th></tr><tr><td>![Protected method](media/protmethod.gif "Protected method")</td><td><a href="3ec01397-075d-3e67-fd3b-2e5c3f149aa2">NotifyDictionaryChangedEventArgs(TKey, TValue)()</a></td><td>
Create an instance describing a <a href="2dae77bf-03d6-02df-4c8e-e1e5ea46a86a">Reset</a> action.</td></tr><tr><td>![Protected method](media/protmethod.gif "Protected method")</td><td><a href="4cde9c7f-747a-63ab-f3b7-13eb16474271">NotifyDictionaryChangedEventArgs(TKey, TValue)(IEnumerable(TValue), NotifyDictionaryChangedAction)</a></td><td>
Initializes a new instance of the NotifyDictionaryChangedEventArgs(TKey, TValue) class</td></tr><tr><td>![Protected method](media/protmethod.gif "Protected method")</td><td><a href="5dbf55fb-ec7b-5019-0478-2b7b96a7f66b">NotifyDictionaryChangedEventArgs(TKey, TValue)(TValue, NotifyDictionaryChangedAction)</a></td><td>
Initializes a new instance of the NotifyDictionaryChangedEventArgs(TKey, TValue) class</td></tr></table>&nbsp;
<a href="#notifydictionarychangedeventargs(*tkey*,-*tvalue*)-class">Back to Top</a>

## Properties
&nbsp;<table><tr><th></th><th>Name</th><th>Description</th></tr><tr><td>![Public property](media/pubproperty.gif "Public property")</td><td><a href="04887103-2749-c463-2e19-cf9588a5be56">Action</a></td><td>
Get the nature of the change to the <a href="b95e4b9e-1bee-ddc0-1db7-61a35069e23a">ObservableDictionary(TKey, TValue)</a> instance.</td></tr><tr><td>![Public property](media/pubproperty.gif "Public property")</td><td><a href="1abb2ce4-d616-eef1-7a65-cbed78c73599">Items</a></td><td>
Get the items involved in the change to the <a href="b95e4b9e-1bee-ddc0-1db7-61a35069e23a">ObservableDictionary(TKey, TValue)</a> instance</td></tr></table>&nbsp;
<a href="#notifydictionarychangedeventargs(*tkey*,-*tvalue*)-class">Back to Top</a>

## Methods
&nbsp;<table><tr><th></th><th>Name</th><th>Description</th></tr><tr><td>![Public method](media/pubmethod.gif "Public method")</td><td>Equals</td><td>
Determines whether the specified object is equal to the current object.
 (Inherited from Object.)</td></tr><tr><td>![Protected method](media/protmethod.gif "Protected method")</td><td>Finalize</td><td>
Allows an object to try to free resources and perform other cleanup operations before it is reclaimed by garbage collection.
 (Inherited from Object.)</td></tr><tr><td>![Public method](media/pubmethod.gif "Public method")</td><td>GetHashCode</td><td>
Serves as the default hash function.
 (Inherited from Object.)</td></tr><tr><td>![Public method](media/pubmethod.gif "Public method")</td><td>GetType</td><td>
Gets the Type of the current instance.
 (Inherited from Object.)</td></tr><tr><td>![Protected method](media/protmethod.gif "Protected method")</td><td>MemberwiseClone</td><td>
Creates a shallow copy of the current Object.
 (Inherited from Object.)</td></tr><tr><td>![Public method](media/pubmethod.gif "Public method")</td><td>ToString</td><td>
Returns a string that represents the current object.
 (Inherited from Object.)</td></tr></table>&nbsp;
<a href="#notifydictionarychangedeventargs(*tkey*,-*tvalue*)-class">Back to Top</a>

## See Also


#### Reference
<a href="bcdbab9c-63d1-48a4-6937-af53fb8d9a55">WetHatLab.OneNote.TaggingKit.common Namespace</a><br />