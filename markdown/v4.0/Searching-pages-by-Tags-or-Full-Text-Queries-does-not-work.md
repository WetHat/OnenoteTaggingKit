# Symptoms

Any combination of the behaviors below:

* No refinement tags shown on the _Find Pages_ dialog.
* Full-text queries on the _Find Pages_ dialog do not work
* No tags shown on the _Suggested Tags_ tab on the _Manage Settings_ dialog 

# Cause

One of the Office 365 updates around April/May broke the integration of _OneNote_32-bit with the _Windows Search Service_ (A defect report has been sent to Microsoft). The _Tagging Kit_ uses the
_Windows Search Service_ to find pages by tags and full text queries. Any search for _OneNote_items using the _Windows Search Service_ returns no results

# Solution

Since version *3.1* the _Tagging Kit_ implements an alternative method for collecting tagged pages without querying the _Windows Search Service_. Make sure the _Use Windows Search Service to find tagged pages_ (2) is **unchecked** on the _Manage Settings Dialog_ to use that alternative method:

>![Preferences Tab](https://github.com/WetHat/OnenoteTaggingKit/wiki/images/PreferencesTab.png)

The alternative method is considerably slower than a _Windows Search Service_ query and does not work for full-text search with the _Find Pages_ dialog at all. Until Microsoft fixes the _Windows Search Service_ integration, full-text search is not available through the _Find pages_ dialog. For full-text search to come back a fix from Microsoft is needed.
