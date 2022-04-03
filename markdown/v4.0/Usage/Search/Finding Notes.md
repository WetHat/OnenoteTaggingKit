# Finding _OneNote_ Pages

The _Find Pages_ dialog is used to find _OneNote_ pages by applying tag and
full text filters.

![Find Pges Button](images/RibbonFind.png)

To acivate the _Fins Pages_ dialog click or tap on the _Fins Pages_
button of the _Page Tags_ group in the `Home` tab of the _OneNote_ ribbon.

# The _Find Pages_ Dialog

:point_up: Numbered dialog elements are referenced by superscripts and list item
indices.

![Tag Pages Dialog](images/TagPagesDialog.png){.rightfloat}

1. The search scope. Only pages in the selected scope will be included in the
   search result. Available scopes are:{id="Dia-1"}

   `This Section`
   :   Only pages from the current section are included in the search result.

   `This Section Group`
   :    Only pages from the current section group are included in the
        search result.

   `This Notebook`
   :   Only pages from the current notebook are included in the
        search result.

   `All Notebooks`
   :   Pages from all notebooks currently open in OneNote are included in the
       search result.
2. Search query input box for full text search quavailable while focus is on the
   query input box:{id="Dia-2"}

    `ESC`
    :   Clear the query input box. To update the query tap or click the search
        button[^3^](#Dia-3).

    `ENTER`
    :   Perform a full text search using the entered search terms.
       Same as clicking on the _search_ button[^3^](#Dia-3)
3. Perform a full text search using the terms in the query input
   box[^2^](#Dia-2). The search result is displayed in the _Pages_
   panel[^14^](Dia-14). Matches of search terms with page titles are highlighted
   [^16^](#Dia-16). Only pages in the selected scope[^1^](#Dia-1) are
   shown.{id="Dia-3"}
4. A tag selected for refinement. To remove the tag from the filter, click or
   tap on it.{id="Dia-4"}
5. The  _Refinement Tags Panel_. The number in parentheses after the panel title
   indicates the number of tags selected for refinement.{id="Dia-5"}
6. Clears all currently selected refinement tags.{id="Dia-6"}
7. Tag Filter Presets. Filters the collection of tags currently available in the
   _Page Tags_ panel[^14^](#Dia-14) using tags found in the selected range.
   The tag presets are entered into the
   tag input box[^8^](#Dia-8) as comma separated list and can be used to update
   the page search result by clicking the _Select all matching tags_ button
   [^11^](#Dia-11) to refine the search result[^14^](#Dia-14).{id="Dia-6"}
8. Tag filter input box. Enter one or more tagnames (comma ',' separated) to
   to show only tags in the _Tags_ panel[^14^](#Dia-14) which match any of the
   typed tag names.
   The collection of tags in the _Page Tags_ panel[^14^](#Dia-14) is updated
   as you type.{id="Dia-8"}

   Following keyboard shortcuts are supported while focus is on the
   input box:

   `ESC`
   :   Clear the input box. Same as pressing the _Clear_ button[^9^](#Dia-9).

   `SHIFT`+`ESC`
   :   Clear the tag input box and also clear all currently selected refinement
       tags[^5^](#Dia-5) (Same as pressing the _Clear_ button[^9^](#Dia-9)) **and**
       also the _Clear_ button[^6^](#Dia-6).

   `ENTER`
   :   Select all tags from the collection of tags in the _Page Tags_ panel[^10^](#Dia-10)
       which fully match one of the entered tag names to happen to fully match a tag.
       The collection of refinement tags in the _Refinement Tags_ panel[^5^](#Dia-5)
       and the list of found pages[^14^](#Dia-14) are updated accordingly.
9. Clear the current tag filter entered in the tag filter input box[^8^](#Dia-8).{id="Dia-9"}
10. _Page Tags_ panel. The collection of tags available for refinement. If this
    collection is inconveniently large large, enter a tag filter into the tag filter input box
    [^8^](#Dia-8) to show only matching tags.{id="Dia-10"}
11. Select fully matching tags. Selects all tags from the _Page Tags_[^10^](#Dia-10)
    panel which fully match one of the tags entered in the tag filter input
    box[^8^](#Dia-8).{id="Dia-11"}
12. A tag available for search result refinement. This tag is an imported hashtag
    and matches the partial tag name entered in the tag filter input box[^8^](#Dia-8).
    A tap or click on it selects it for refinement[^5^](#Dia-5) and updates the search result
    [^14^](#Dia-14).{id="Dia-12"}
13. A genuine _Page Tag_ (not imported) available for search result refinement.
    The tag matches the partial tag name entered in the tag filter input box[^8^](#Dia-8).
    A tap or click on it selects it for refinement[^5^](#Dia-5) and updates the search result
    [^14^](#Dia-14).{id="Dia-13"}
14. The _Pages_ search result panel. Displays all pages matching the refinement
    tag filter[^5^](#Dia-5) and the full text query specified in[^2^](#Dia-2).
    The Panel header shows additional status information:{id="Dia-14"}

    * The number in parenthesis after the panel title indicates the number of
      pages matching the search criteria,
    * 🔖 - The number of tags in the refinement filter[^5^](#Dia-5).
    * 🔍 - The full text search query used
15. The _Search Result Action Menu_. Click or tap to access actions for the
    search result[^14^](#Dia-14).{id="Dia-15"}

    The available actions are.
    
    `Refresh`
    :   Refresh the search result using the current collection if refinement
        tags[^5^](#Dia-5) and the current search query. This is sometimes
        needed when pages have been tagged while _Find Pages_ dialog was open.

    `TODO`
16. A link to a page in the search result. Tap or click on the link to navigate
    to the page in _OneNote_. A link can be selected by clicking on the `❱` symbol.
    Several page links can be selected by holding the `CTRL`-key while clicking.
    Holding the `SHIFT`-key while clicking allows selection of ranges of links.
    Link  selections are required for some actions in the _Search Result Action Menu_
    [^15^](#Dia-15).{id="Dia-16"}
17. Activate Tracking Mode. Automatically tracks related pages based on the tags
    on the current page. When tag tracking is enabled:{id="Dia-17"}

    * the current page's title is displayed next to the tag tracking checkbox.
    * the tags on the current _OneNote_ page are extracted and pre-set as
      filter tags[^8^](#Dia-8).
    * The _Select fully matching tags_ action can [^11^](#Dia-11) be executed to
      quickly get all pages with these tags.

# Workflows

TODO




---

2. When the _Find Notes_ dialog window opens, you can find pages by one of
   following strategies:

   ### Basic Strategies
   * [[Find Notes by Tags]] - just using tags to find pages
   * [[Find notes by facetted search|Facetted Search]] - full text search **including** tags.

   ### Advanced Strategies
   * [[Finding Related Notes|Find Related Notes]]
   * [[Working with Refinement Tags]]
   * [[Working with the Search Result]]