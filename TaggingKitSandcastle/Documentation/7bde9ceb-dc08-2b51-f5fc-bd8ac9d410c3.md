# NamedObjectBase Class
 _**\[This is preliminary documentation and is subject to change.\]**_

Base class for proxy objeccts containing a XML element with a `name` attribute.


## Inheritance Hierarchy
System.Object<br />&nbsp;&nbsp;<a href="10522ffc-023c-fe2b-d07f-22ef617cb6f6.md">WetHatLab.OneNote.TaggingKit.PageBuilder.PageObjectBase</a><br />&nbsp;&nbsp;&nbsp;&nbsp;<a href="9614e26d-4f3e-ec75-682e-cd6e5bcdf145.md">WetHatLab.OneNote.TaggingKit.PageBuilder.PageStructureObjectBase</a><br />&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;WetHatLab.OneNote.TaggingKit.PageBuilder.NamedObjectBase<br />&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<a href="01a6f6f8-9cda-e956-272e-3b49a8fafa46.md">WetHatLab.OneNote.TaggingKit.PageBuilder.DefinitionObjectBase</a><br />&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<a href="90c71725-7f0d-fb9a-38b1-3b78c27eea6f.md">WetHatLab.OneNote.TaggingKit.PageBuilder.Meta</a><br />
**Namespace:**&nbsp;<a href="56352230-71f2-f4b7-63a8-983965663af5.md">WetHatLab.OneNote.TaggingKit.PageBuilder</a><br />**Assembly:**&nbsp;OneNoteTaggingKit (in OneNoteTaggingKit.dll) Version: 4.0.8132.30441

## Syntax

**C#**<br />
``` C#
public class NamedObjectBase : PageStructureObjectBase
```

The NamedObjectBase type exposes the following members.


## Constructors
&nbsp;<table><tr><th></th><th>Name</th><th>Description</th></tr><tr><td>![Protected method](media/protmethod.gif "Protected method")</td><td><a href="9786c187-cab5-08f2-848a-9d6322703e5e.md">NamedObjectBase(OneNotePage, XElement)</a></td><td>
Initialize a proxy object with a XML element selected from a OneNote page document.</td></tr><tr><td>![Protected method](media/protmethod.gif "Protected method")</td><td><a href="0cb8d1b1-2727-c6fb-780f-b4f745021fc6.md">NamedObjectBase(OneNotePage, XElement, String)</a></td><td>
Initialize a proxy containing a new XML element.</td></tr></table>&nbsp;
<a href="#namedobjectbase-class">Back to Top</a>

## Properties
&nbsp;<table><tr><th></th><th>Name</th><th>Description</th></tr><tr><td>![Public property](media/pubproperty.gif "Public property")</td><td><a href="b1355277-06a2-7c7b-8423-2a3d979b9e32.md">Element</a></td><td>
Get or set the raw XML element.
 (Inherited from <a href="10522ffc-023c-fe2b-d07f-22ef617cb6f6.md">PageObjectBase</a>.)</td></tr><tr><td>![Public property](media/pubproperty.gif "Public property")</td><td><a href="9298a3af-e6c3-905a-d1c8-8960d9fb4deb.md">Name</a></td><td>
Get/set the name of the element.</td></tr><tr><td>![Public property](media/pubproperty.gif "Public property")</td><td><a href="f3e4f694-8098-5550-71ff-8ae66afd9f7a.md">Namespace</a></td><td>
Get the Namespace the XML element associated with this proxy object exists in.
 (Inherited from <a href="10522ffc-023c-fe2b-d07f-22ef617cb6f6.md">PageObjectBase</a>.)</td></tr><tr><td>![Public property](media/pubproperty.gif "Public property")</td><td><a href="66f538ed-fce0-bfa7-f916-b2a63cf75127.md">Page</a></td><td>
Proxy for the OneNote page document which owns the element contained in this proxy.
 (Inherited from <a href="9614e26d-4f3e-ec75-682e-cd6e5bcdf145.md">PageStructureObjectBase</a>.)</td></tr></table>&nbsp;
<a href="#namedobjectbase-class">Back to Top</a>

## Methods
&nbsp;<table><tr><th></th><th>Name</th><th>Description</th></tr><tr><td>![Public method](media/pubmethod.gif "Public method")</td><td><a href="febd286e-b95d-3257-ffed-d2b4475144e4.md">Equals</a></td><td>
Check decorators for equality.
 (Inherited from <a href="10522ffc-023c-fe2b-d07f-22ef617cb6f6.md">PageObjectBase</a>.)</td></tr><tr><td>![Protected method](media/protmethod.gif "Protected method")</td><td>Finalize</td><td>
Allows an object to try to free resources and perform other cleanup operations before it is reclaimed by garbage collection.
 (Inherited from Object.)</td></tr><tr><td>![Protected method](media/protmethod.gif "Protected method")</td><td><a href="4d9c0f69-ca27-d06d-850a-46da816f98ab.md">GetAttributeValue</a></td><td>
Get the value of an attribute
 (Inherited from <a href="10522ffc-023c-fe2b-d07f-22ef617cb6f6.md">PageObjectBase</a>.)</td></tr><tr><td>![Public method](media/pubmethod.gif "Public method")</td><td><a href="ebe970b7-5320-4551-378d-7958ca5e66fd.md">GetHashCode</a></td><td>
Get the hash code of this decorator.
 (Inherited from <a href="10522ffc-023c-fe2b-d07f-22ef617cb6f6.md">PageObjectBase</a>.)</td></tr><tr><td>![Public method](media/pubmethod.gif "Public method")</td><td><a href="24d1c39f-0f88-8c79-394d-4fc20eaacccb.md">GetName</a></td><td>
Get a fully qualified element name
 (Inherited from <a href="10522ffc-023c-fe2b-d07f-22ef617cb6f6.md">PageObjectBase</a>.)</td></tr><tr><td>![Public method](media/pubmethod.gif "Public method")</td><td>GetType</td><td>
Gets the Type of the current instance.
 (Inherited from Object.)</td></tr><tr><td>![Protected method](media/protmethod.gif "Protected method")</td><td>MemberwiseClone</td><td>
Creates a shallow copy of the current Object.
 (Inherited from Object.)</td></tr><tr><td>![Public method](media/pubmethod.gif "Public method")</td><td><a href="038c07b4-81ab-47d2-e16c-516917687b3a.md">Remove</a></td><td>
Remove XML element from its parent.
 (Inherited from <a href="10522ffc-023c-fe2b-d07f-22ef617cb6f6.md">PageObjectBase</a>.)</td></tr><tr><td>![Protected method](media/protmethod.gif "Protected method")</td><td><a href="77d51981-a0cd-15e4-5ea7-0f1dc3d61657.md">SetAttributeValue</a></td><td>
Set an attribute value
 (Inherited from <a href="10522ffc-023c-fe2b-d07f-22ef617cb6f6.md">PageObjectBase</a>.)</td></tr><tr><td>![Public method](media/pubmethod.gif "Public method")</td><td>ToString</td><td>
Returns a string that represents the current object.
 (Inherited from Object.)</td></tr></table>&nbsp;
<a href="#namedobjectbase-class">Back to Top</a>

## See Also


#### Reference
<a href="56352230-71f2-f4b7-63a8-983965663af5.md">WetHatLab.OneNote.TaggingKit.PageBuilder Namespace</a><br />