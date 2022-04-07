# ObservableObject Class
 _**\[This is preliminary documentation and is subject to change.\]**_

Base class for objects which expose observable properties.


## Inheritance Hierarchy
System.Object<br />&nbsp;&nbsp;WetHatLab.OneNote.TaggingKit.common.ObservableObject<br />&nbsp;&nbsp;&nbsp;&nbsp;<a href="8abe04f4-0682-74c0-5557-fa48d6eff35f">WetHatLab.OneNote.TaggingKit.common.TagPageSet</a><br />&nbsp;&nbsp;&nbsp;&nbsp;<a href="c74fe645-91b2-831c-6869-763addf746aa">WetHatLab.OneNote.TaggingKit.common.ui.TagModel</a><br />
**Namespace:**&nbsp;<a href="bcdbab9c-63d1-48a4-6937-af53fb8d9a55">WetHatLab.OneNote.TaggingKit.common</a><br />**Assembly:**&nbsp;OneNoteTaggingKit (in OneNoteTaggingKit.dll) Version: 4.0.8132.30441

## Syntax

**C#**<br />
``` C#
[ComVisibleAttribute(false)]
public class ObservableObject : INotifyPropertyChanged, 
	IDisposable
```

The ObservableObject type exposes the following members.


## Constructors
&nbsp;<table><tr><th></th><th>Name</th><th>Description</th></tr><tr><td>![Public method](media/pubmethod.gif "Public method")</td><td><a href="b0f56fd6-bd5a-7622-f275-0858fceec42f">ObservableObject</a></td><td>
Initializes a new instance of the ObservableObject class</td></tr></table>&nbsp;
<a href="#observableobject-class">Back to Top</a>

## Methods
&nbsp;<table><tr><th></th><th>Name</th><th>Description</th></tr><tr><td>![Public method](media/pubmethod.gif "Public method")</td><td><a href="35d00535-1e7e-22a1-cb53-7637d411dec7">Dispose</a></td><td>
Clear all property handlers for this object.</td></tr><tr><td>![Public method](media/pubmethod.gif "Public method")</td><td>Equals</td><td>
Determines whether the specified object is equal to the current object.
 (Inherited from Object.)</td></tr><tr><td>![Protected method](media/protmethod.gif "Protected method")</td><td>Finalize</td><td>
Allows an object to try to free resources and perform other cleanup operations before it is reclaimed by garbage collection.
 (Inherited from Object.)</td></tr><tr><td>![Public method](media/pubmethod.gif "Public method")</td><td>GetHashCode</td><td>
Serves as the default hash function.
 (Inherited from Object.)</td></tr><tr><td>![Public method](media/pubmethod.gif "Public method")</td><td>GetType</td><td>
Gets the Type of the current instance.
 (Inherited from Object.)</td></tr><tr><td>![Protected method](media/protmethod.gif "Protected method")</td><td>MemberwiseClone</td><td>
Creates a shallow copy of the current Object.
 (Inherited from Object.)</td></tr><tr><td>![Protected method](media/protmethod.gif "Protected method")![Code example](media/CodeExample.png "Code example")</td><td><a href="5d0bdc82-8ecd-785e-4513-483e68b3fbe6">RaisePropertyChanged</a></td><td>
Raise a change event for</td></tr><tr><td>![Public method](media/pubmethod.gif "Public method")</td><td>ToString</td><td>
Returns a string that represents the current object.
 (Inherited from Object.)</td></tr></table>&nbsp;
<a href="#observableobject-class">Back to Top</a>

## Events
&nbsp;<table><tr><th></th><th>Name</th><th>Description</th></tr><tr><td>![Public event](media/pubevent.gif "Public event")</td><td><a href="185ee554-4bcc-0dd9-592a-42256ef46b35">PropertyChanged</a></td><td>
Event to notify registered handlers about property changes</td></tr></table>&nbsp;
<a href="#observableobject-class">Back to Top</a>

## Remarks
To support working with view models in background tasks the use of dependency objects is not recommended because of their thread affinity. Instead observable properties should raise PropertyChanged events. Methods to do that in a type save way are provided in this base class.

## See Also


#### Reference
<a href="bcdbab9c-63d1-48a4-6937-af53fb8d9a55">WetHatLab.OneNote.TaggingKit.common Namespace</a><br />