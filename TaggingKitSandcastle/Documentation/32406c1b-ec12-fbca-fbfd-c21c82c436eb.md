# RemovableTagModel Class
 _**\[This is preliminary documentation and is subject to change.\]**_

View model backing a <a href="ef583703-d11c-ba42-c90f-7c19350d1e2b">RemovableTag</a> user control.


## Inheritance Hierarchy
<a href="http://msdn2.microsoft.com/en-us/library/e5kfa45b" target="_blank">System.Object</a><br />&nbsp;&nbsp;<a href="fc433c94-8fb7-e877-217c-2bcf31c00339">WetHatLab.OneNote.TaggingKit.common.SuggestedTagDataContext</a><br />&nbsp;&nbsp;&nbsp;&nbsp;WetHatLab.OneNote.TaggingKit.manage.RemovableTagModel<br />
**Namespace:**&nbsp;<a href="6c09c3a7-2ecd-33d5-2ed0-acefd996500f">WetHatLab.OneNote.TaggingKit.manage</a><br />**Assembly:**&nbsp;OneNoteTaggingKit (in OneNoteTaggingKit.dll) Version: 3.2.6524.26703

## Syntax

**C#**<br />
``` C#
public class RemovableTagModel : SuggestedTagDataContext
```

The RemovableTagModel type exposes the following members.


## Constructors
&nbsp;<table><tr><th></th><th>Name</th><th>Description</th></tr><tr><td>![Public method](media/pubmethod.gif "Public method")</td><td><a href="5235a1df-2564-7acd-7bf3-912397a924a9">RemovableTagModel</a></td><td>
Create a new instance of the view model.</td></tr></table>&nbsp;
<a href="#removabletagmodel-class">Back to Top</a>

## Properties
&nbsp;<table><tr><th></th><th>Name</th><th>Description</th></tr><tr><td>![Public property](media/pubproperty.gif "Public property")</td><td><a href="5cfc131f-3df4-9535-21fa-d17745787ad7">CanRemove</a></td><td>
Check whether the tag can be removed</td></tr><tr><td>![Public property](media/pubproperty.gif "Public property")</td><td><a href="d2650e51-f6f6-1af5-8400-e7aa12223097">HasHighlights</a></td><td>
Determine if the suggested tag's name has highlights.
 (Inherited from <a href="fc433c94-8fb7-e877-217c-2bcf31c00339">SuggestedTagDataContext</a>.)</td></tr><tr><td>![Protected property](media/protproperty.gif "Protected property")</td><td><a href="8b6d9444-c7e9-e673-7bb8-8ff5f63f7226">HighlightedTagName</a></td><td> (Inherited from <a href="fc433c94-8fb7-e877-217c-2bcf31c00339">SuggestedTagDataContext</a>.)</td></tr><tr><td>![Public property](media/pubproperty.gif "Public property")</td><td><a href="c9b07260-331a-8f63-d120-26e3604c9662">Highlighter</a></td><td>
Set a filter string which is used to determine the appearance of the <a href="e0797c9e-c150-c273-e1aa-98d5d25e1ee1">HitHighlightedTagButton</a> control.
 (Inherited from <a href="fc433c94-8fb7-e877-217c-2bcf31c00339">SuggestedTagDataContext</a>.)</td></tr><tr><td>![Public property](media/pubproperty.gif "Public property")</td><td><a href="3d100a09-3b36-492c-a8fb-7c93fd3a97f5">Key</a></td><td>
Get the unique key of the data context
 (Inherited from <a href="fc433c94-8fb7-e877-217c-2bcf31c00339">SuggestedTagDataContext</a>.)</td></tr><tr><td>![Public property](media/pubproperty.gif "Public property")</td><td><a href="ecdd57d2-f93d-815e-3b09-01670bed67c3">LocalName</a></td><td /></tr><tr><td>![Public property](media/pubproperty.gif "Public property")</td><td><a href="354a04fb-11c5-b386-8676-4a5f156e8554">SortKey</a></td><td>
Get the sortable key of the data context.
 (Inherited from <a href="fc433c94-8fb7-e877-217c-2bcf31c00339">SuggestedTagDataContext</a>.)</td></tr><tr><td>![Protected property](media/protproperty.gif "Protected property")</td><td><a href="f13dd952-ee76-bf63-e6ca-abc238060d35">Tag</a></td><td>
Get or set the Tag for the view model.</td></tr><tr><td>![Protected property](media/protproperty.gif "Protected property")</td><td><a href="0ab61a62-1ee3-ae78-9fc0-3f413a699534">TagName</a></td><td> (Inherited from <a href="fc433c94-8fb7-e877-217c-2bcf31c00339">SuggestedTagDataContext</a>.)</td></tr><tr><td>![Public property](media/pubproperty.gif "Public property")</td><td><a href="795e5c87-fcf0-59bc-725c-d489a3892dff">UseCount</a></td><td>
Get the number of pages having this tag.</td></tr><tr><td>![Public property](media/pubproperty.gif "Public property")</td><td><a href="5e1dc59b-3a9d-e275-970d-f20188539b36">UseCountColor</a></td><td>
Get the color of the tag use count indicator.</td></tr></table>&nbsp;
<a href="#removabletagmodel-class">Back to Top</a>

## Methods
&nbsp;<table><tr><th></th><th>Name</th><th>Description</th></tr><tr><td>![Public method](media/pubmethod.gif "Public method")</td><td><a href="http://msdn2.microsoft.com/en-us/library/bsc2ak47" target="_blank">Equals</a></td><td> (Inherited from <a href="http://msdn2.microsoft.com/en-us/library/e5kfa45b" target="_blank">Object</a>.)</td></tr><tr><td>![Protected method](media/protmethod.gif "Protected method")</td><td><a href="http://msdn2.microsoft.com/en-us/library/4k87zsw7" target="_blank">Finalize</a></td><td> (Inherited from <a href="http://msdn2.microsoft.com/en-us/library/e5kfa45b" target="_blank">Object</a>.)</td></tr><tr><td>![Protected method](media/protmethod.gif "Protected method")</td><td><a href="73cab1d0-b594-f2c1-f800-1bae260f2de9">firePropertyChanged</a></td><td>
Fire a PropertyChanged event
 (Inherited from <a href="fc433c94-8fb7-e877-217c-2bcf31c00339">SuggestedTagDataContext</a>.)</td></tr><tr><td>![Public method](media/pubmethod.gif "Public method")</td><td><a href="http://msdn2.microsoft.com/en-us/library/zdee4b3y" target="_blank">GetHashCode</a></td><td> (Inherited from <a href="http://msdn2.microsoft.com/en-us/library/e5kfa45b" target="_blank">Object</a>.)</td></tr><tr><td>![Public method](media/pubmethod.gif "Public method")</td><td><a href="http://msdn2.microsoft.com/en-us/library/dfwy45w9" target="_blank">GetType</a></td><td> (Inherited from <a href="http://msdn2.microsoft.com/en-us/library/e5kfa45b" target="_blank">Object</a>.)</td></tr><tr><td>![Protected method](media/protmethod.gif "Protected method")</td><td><a href="http://msdn2.microsoft.com/en-us/library/57ctke0a" target="_blank">MemberwiseClone</a></td><td> (Inherited from <a href="http://msdn2.microsoft.com/en-us/library/e5kfa45b" target="_blank">Object</a>.)</td></tr><tr><td>![Public method](media/pubmethod.gif "Public method")</td><td><a href="http://msdn2.microsoft.com/en-us/library/7bxwbwt2" target="_blank">ToString</a></td><td> (Inherited from <a href="http://msdn2.microsoft.com/en-us/library/e5kfa45b" target="_blank">Object</a>.)</td></tr></table>&nbsp;
<a href="#removabletagmodel-class">Back to Top</a>

## Events
&nbsp;<table><tr><th></th><th>Name</th><th>Description</th></tr><tr><td>![Public event](media/pubevent.gif "Public event")</td><td><a href="3e2dfcff-6656-e61a-cfd1-8a846b917edf">PropertyChanged</a></td><td>
Event to notify subscribers about property changes.
 (Inherited from <a href="fc433c94-8fb7-e877-217c-2bcf31c00339">SuggestedTagDataContext</a>.)</td></tr></table>&nbsp;
<a href="#removabletagmodel-class">Back to Top</a>

## Fields
&nbsp;<table><tr><th></th><th>Name</th><th>Description</th></tr><tr><td>![Protected field](media/protfield.gif "Protected field")![Static member](media/static.gif "Static member")</td><td><a href="fe9da3f9-ead0-c8d3-36ca-44ddb31be028">CAN_REMOVE</a></td><td /></tr><tr><td>![Protected field](media/protfield.gif "Protected field")![Static member](media/static.gif "Static member")</td><td><a href="0dbd218f-a525-15af-6a58-5009127278e0">LOCAL_NAME</a></td><td /></tr><tr><td>![Protected field](media/protfield.gif "Protected field")![Static member](media/static.gif "Static member")</td><td><a href="52e717c8-caac-4aa8-9482-dcdd2380ab83">MARKER_VISIBILIY</a></td><td /></tr><tr><td>![Protected field](media/protfield.gif "Protected field")![Static member](media/static.gif "Static member")</td><td><a href="637ea74d-d6d6-dfc1-d759-2ad83f9ed5af">USE_COUNT</a></td><td /></tr><tr><td>![Protected field](media/protfield.gif "Protected field")![Static member](media/static.gif "Static member")</td><td><a href="6da4d067-78e9-1b2b-6ca6-d0afe7e4594d">USE_COUNT_COLOR</a></td><td /></tr></table>&nbsp;
<a href="#removabletagmodel-class">Back to Top</a>

## Remarks
Provides properties to enable/disable a tag for removal and to adjust the presentation of the corresponding UI element.

## See Also


#### Reference
<a href="6c09c3a7-2ecd-33d5-2ed0-acefd996500f">WetHatLab.OneNote.TaggingKit.manage Namespace</a><br />