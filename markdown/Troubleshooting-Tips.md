# Troubleshooting the TaggingKit{.title}

If the issue you are having is not already covered below, please also check the
[Issues](https://github.com/WetHat/OnenoteTaggingKit/issues) page on GitHub.
Your issue may already heve been reported or fixed.

## Common Known Issues {.unfloat}

* [[The Tagging Kit command group does not show up in the _OneNote_ribbon bar|Tagging Kit Not Shown in Ribbon]]
* [[Searching pages by Tags or Full-Text Queries does not work]]
* [[Exception from HRESULT: 0x8004200C|0x8004200C]]
* [[Exception from HRESULT: 0x80042019|0x80042019]]

## Submitting a New Issue or Enhancement Request

![Screenshot](images/TroubleshootingTips_log.png){.rightfloat}

You are also welcome to submit new issues or enhancement requests, if nothing
appropriate is recorded at
[Issues](https://github.com/WetHat/OnenoteTaggingKit/issues)
    
Make sure you **always** attach the add-in's logfile.
The easiest way to locate the logfile is to open  the `Settings` dialog and
select the `About` tab.

Alternatively you can open File Explorer, navigate to the
`%TEMP%` directory and pick up the **newest** logfile named `taggingkit_*.log`.
The `*` stands for a sequence of characters and numbers, e.g.
`taggingkit_8DA0C2A5B55670C.log`. 
