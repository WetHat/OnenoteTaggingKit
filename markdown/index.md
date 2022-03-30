# _OneNote Tagging Kit_ Add-In{.title}

A free to-use, add-in to add page tagging to _OneNote 
for the desktop_. The add-in is hosted as _Open Source_ project on
[GitHub](https://github.com/WetHat/OnenoteTaggingKit).

# Feature Overview

* Unlike the build-in OneNote paragraph tags, _Tagging Kit Page Tags_ mark an
  entire page. The _OneNote Tagging Kit_ utilizes these page tags to provide
  an enhanced search experience which allows to search for pages by
  [Faceted Search](https://en.wikipedia.org/wiki/Faceted_search).
  Search results can be dynamically refined by adding _Page Tags_ or full text
  searches to the filter.
* The Add-In provides several dialogs to apply, manage, and search with page tags.
* Tagging operations are performed in the background. _OneNote_ can be used
  normally while tagging is in progress. 
* Page Tags are based on _OneNote_ paragraph tags and are automatically shared
  to all connected OneNote clients. If a _OneNote_ client does not have the
  _Tagging Kit_ installed, it still has access to the page tags via
  the built-in _OneNote_ tagging system. If a connected _OneNote_ client has the
  _Tagging Kit_ add-in, full tagging functionality is available.

Version specific features are covered in the [release specific](#releases) 
documentation of the add-in.

# About Tagging

[The Use of Tags](Use%20of%20Tags.md) briefly outlines the concept behind tags
for information management.

Some thoughts about the use of _Page Tags_ to organize notes in _OneNote_ can
be found in [Organizing Notes with Page Tags](Organizing%20Notes%20with%20Page%20Tags.md).

# Getting Started with the _OneNote Tagging Kit_ Add-In

Installation and use of the _Tagging Kit_ can be found in the [release
specific](#releases) documentation.

# Bugs and Features

![Screenshot](images/TroubleshootingTips_log.png){.rightfloat}
 
You are very much welcome to submit new issues or enhancement requests at
[Issues](https://github.com/WetHat/OnenoteTaggingKit/issues). Please avoid
duplicates and check if the issue you are going to submit has already been
reported or fixed. Also, take a look at the troubleshooting
tips in the [release specific](#releases) documentation.
    
Make sure you **always** attach the add-in's logfile when submitting bugs.
The easiest way to locate the logfile is to open  the `Settings` dialog and
select the `About` tab.

Alternatively you can open File Explorer, navigate to the
`%TEMP%` directory and pick up the **newest** logfile named `taggingkit_*.log`.
The `*` stands for a sequence of characters and numbers, e.g.
`taggingkit_8DA0C2A5B55670C.log`. 

# Releases{.unfloat}

🌟 **v4.0** - [Documentation](v4.0/Home.md) - [Release Notes &amp; Download] 
  
