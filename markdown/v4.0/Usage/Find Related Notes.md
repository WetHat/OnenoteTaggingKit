# Finding Related Notes

To find related notes it is sometimes more convenient to use a more lenient way to filter pages
than progressive refinement. The progressive refinement approach is based on the _intersection_ of
page sets having all the selected tags. The resulting pages have **all** of the selected tags.
This is an efficient way to quickly reduce the number of pages in a search result,
particularly if multiple tags are selected.

In some cases, however, it is preferable to filter for pages which have **any** of a set of tags.
There is no direct implementation of such a method, but something similar can be achieved 
by using one of the methods described below.

## Finding Related Notes Manually

![Refinement Tags](https://github.com/WetHat/OnenoteTaggingKit/wiki/images/FacettedSearch.png)  

1. We define a search scope by selecting one of the pre-difined scopes from **(1)**.
2. Optionally we define an additional search criterion in **(2)**
3. We use one of the preset tag filter **(1)** or a manually entered filter **(2)** as described in
   [[Working with Refinement Tags]] to show only tags we are interested in. Note that the number
   displayed for each tag denotes the number of pages having that tag. This approach is somewhat
   similar to a **any** combination of tags. However, since tags are selected by name substring
   matching rather than tag selection, this method casts a wider net.
   
4. We use one of the refinement methods described in [[Find Notes by Tags]] or [[Facetted-Search]] to
   focus on a subset of pages.

## Finding Related Notes Atomatically

![Find Related Notes](https://github.com/WetHat/OnenoteTaggingKit/wiki/images/FindRelatedNotes.png)

When _tag tracking_ **(2)** is enabled:

* The current note's title is displayed next to the _tag tracking_ **(2)** checkbox.
* The tags on the current _OneNote_page are extracted and pre-set as filter tags **(1)**.
  This is equivalent to using the _Tags from Current Page_ filter preset as described
  in [Working with Refinement Tags](WorkingWithRefinementTags).
  However, with  _tag tracking_ **(2)** enabled the tag filter is updated automatically
  whenever the current page changes.

To locate pages related to the current page you can use any of the
search methods described in [[Find Notes by Tags]]]
or [[Find notes by facetted search]|Facetted-Search]] 