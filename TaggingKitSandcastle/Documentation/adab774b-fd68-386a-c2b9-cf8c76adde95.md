# TaggingJob.Execute Method 
 _**\[This is preliminary documentation and is subject to change.\]**_

Tag a singe OneNote page

**Namespace:**&nbsp;<a href="bf353949-2ab8-bf1a-9a78-ce64949f480c.md">WetHatLab.OneNote.TaggingKit.Tagger</a><br />**Assembly:**&nbsp;OneNoteTaggingKit (in OneNoteTaggingKit.dll) Version: 4.0.8135.22136

## Syntax

**C#**<br />
``` C#
internal OneNotePage Execute(
	OneNoteProxy onenote,
	OneNotePage page
)
```


#### Parameters
&nbsp;<dl><dt>onenote</dt><dd>Type: <a href="a46a793f-b110-250f-657a-ecb64aa3bbf7.md">WetHatLab.OneNote.TaggingKit.OneNoteProxy</a><br />OneNote application proxy object</dd><dt>page</dt><dd>Type: <a href="6754c7d7-0598-ae1f-ff8c-6808b714b0ab.md">WetHatLab.OneNote.TaggingKit.PageBuilder.OneNotePage</a><br />an unsaved OneNote page which has been tagged previously</dd></dl>

#### Return Value
Type: <a href="6754c7d7-0598-ae1f-ff8c-6808b714b0ab.md">OneNotePage</a><br />Unsaved, tagged OneNote page.

## Remarks
A tagged page is not saved immediately. The caller can hold on to a previously returned page and pass it into this method again. This avoids saving a page multiple times, if there are subsequent tagging jobs for the same page. If the ID of the page passed into this method does not match the ID of this job, the passed in page is saved.

## See Also


#### Reference
<a href="447270ca-da51-967b-5344-b56c928c5068.md">TaggingJob Class</a><br /><a href="bf353949-2ab8-bf1a-9a78-ce64949f480c.md">WetHatLab.OneNote.TaggingKit.Tagger Namespace</a><br />