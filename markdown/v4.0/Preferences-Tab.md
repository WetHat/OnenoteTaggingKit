# The _Preferences_ Tab

![Preferences Tab](images/PreferencesTab.png)
1. In the _Dialog Preferences_ section (1) you can disable or enable
   the _always on top_ behavior of the _Tag Pages_ or _Find Tagged Pages_ dialog.

2. The _Operating System Integration_ section (2) allows select or deselect the
   _Windows Search Service_ for finding page tags. By default this option is **on**.

   When this option is unchecked a slower, alternative approach for
   finding page tags is used. Unchecking this option may be necessary on systems where the _OneNote_/ _Windows Search Service_
   integration is broken (this happened recently when an _OneNote_upgrade broke the search service integration due to a bug
   in the installer).

3. In _Tag Options_ section (3) you can configure format and display
   of page tags on a _OneNote_page:

   * Tag Formats:

     * **Capitalized** - The first character of each tag is changed to uppercase. If a
       tag has multiple words separated by space, the first character of each word is
       changed to uppercase.

       For example: Tag input _documentation_ is changed to _Documentation_

     * **HashTag** - The tag is formatted as a [hashtag](https://en.wikipedia.org/wiki/Hashtag),
       by placing the number sign or pound sign # (also known as the hash character) in front of
       a tag word or unspaced phrase.

       For example tag input _Documentation_ is changed to _#documentation_

     * **As Entered** - The tag entered by the user is used _as typed_. No formatting is applied.
       Multi-word tags can be separated by spaces.

   * Tag Display:

     * **Below Page Title** - Tags are displayed below the title of a
       _OneNote_page:

       ![Page Tags](https://github.com/WetHat/OnenoteTaggingKit/wiki/images/TaggedPage.png)

        **Note!** The tags are represented as editable text, but edits do **not** change the actual page tags.
        To change page tags the [Tag Pages](https://github.com/WetHat/OnenoteTaggingKit/wiki/Tagging-a-Page)
        dialog must be used.

     * **Icon in Page Title** - A single _OneNote_tag is added to the page title:

       ![Title Tag](https://github.com/WetHat/OnenoteTaggingKit/wiki/images/TitleTag.png)

       To see the page tags you need to hover the mouse over the Tag icon.

       This option has been introduced to address negative side effects in
       the _OneNote_Windows Store version which would display the page tags in
       summary view:

       ![Bad Summary](https://github.com/WetHat/OnenoteTaggingKit/wiki/images/OneNoteUWPSummary1.png)

       instead of a page summary. **Note**: The _OneNote_Windows Store version does
       not display summary views by default! It needs to be enabled in _Settings_:

       ![Summary View Setting](https://github.com/WetHat/OnenoteTaggingKit/wiki/images/OneNotePreview.png)

   **Note**: Tag formatting changes are applied to new tags only.
   Tags existing prior to changing these options remain unchanged.

4. Tag options section **(4)** configures the mapping of tags, which are embedded in
   page content, to page tags. Promoted tags can be used in facetted search. However, if either one
   of the promoted tag types is changed on a page they have to be resynchronized manually.
   See the note on [[Tagging a Page]] on how to use the _re-sync_ button to do that.

   Following options to promote tags from page content to page tags are available.

   * **Map _OneNote_Tags to Page Tags**:

     With every tagging operation the page content is scanned for _OneNote_tags. Each tag found
     on the page is mapped to a corresponding page tag. Mapped page tags originating from _OneNote_tags
     have a special text marker appended, so that they can be distinguished from genuine page tags:

     ![Mapped _OneNote_Tags](https://github.com/WetHat/OnenoteTaggingKit/wiki/images/MappedOneNoteTags.png)

     Mapped _OneNote_tags can be used in the [[Find Notes]] dialog for tag based refinement of the
     search result. However, they do **not** show in tag suggestions because they are not managed
     by the _OneNote Tagging Kit_.

   * **Map Hashtags in Page Text to Page Tags**:

     With every tagging operation the page content is scanned for hashtags embedded in the page text.
     Each hashtag is mapped to a corresponding page tag. Mapped page tags originating from
     hashtags have a special text marker appended so that they can be distinguished from genuine page
     tags:

     ![Mapped Hashtags](https://github.com/WetHat/OnenoteTaggingKit/wiki/images/MappedHashtags.png)

     Mapped hashtags can be used in the [[Find Notes]] dialog for tag based refinement of the
     search result. However, they do **not** show in tag suggestions because they are not managed by the
     _OneNote Tagging Kit_.

   **Note**: Changes to these options do not take effect on existing _OneNote_pages until they are
   tagged again or re-synchronized. See [[Tagging a Page]] on tagging and re-synchronization.


