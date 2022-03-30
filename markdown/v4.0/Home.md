# OnenoteTaggingKit Documentation{.title}

The _OneNoteTaggingKit_ is a free to-use add-in to add page tagging to
OneNote for the desktop.

# Feature Summary:

![Screenshot](https://github.com/WetHat/OnenoteTaggingKit/wiki/images/TaggingKitIntro.png){.rightfloat height="30%"}

* Page Tags can be added/removed from pages on-the-fly
* Search refinement using page tags (facetted search)
* Page tags are available and fully operational for all users of shared _OneNote_notebooks (requires add-in to be installed on _OneNote_clients)
* Full compatibility with built-in _OneNote_tagging system, even if add-in is not installed.
* Locating notes related to the _OneNote_page currently being viewed

# System Requirements

* _OneNote_ 2010, 2013, 2016, 2019 or later, _OneNote_ 365 Desktop (from Office 365).
  🛑 The Windows App Store version is **not** supported.
* Windows 7, 8, 8.1, 10 (32-bit/64-bit); Windows Server 2008 R2 or later
* .net 4.5

To instructions on how to check the _OneNote_ version refer
to [OneNote Version Check](OneNote-Version-Check.md)

# Installation and Upgrade

The add-in can be installed or upgraded in one of the following ways:

## Direct Installation / Upgrade
>
> 1. Verify that the system requirements are met.
> 2. Download the installer (*.msi) from [Releases](https://github.com/WetHat/OnenoteTaggingKit/releases)
> 3. Make sure _OneNote_ is **not running**! Exit _OneNote_ if necessary.
> 4. For first time installation and upgrade execute the downloaded installer (*.msi)
> 5. Start OneNote.

## Installation / Upgrade via the [Chocolatey Package Manager](https://community.chocolatey.org/packages/onenote-taggingkit-addin.install)
> [Chocolatey](https://chocolatey.org/) can be [installed here](https://chocolatey.org/install).
> If you are wondering why you would want to use Chocolatey,
> [here is why](https://chocolatey.org/why-chocolatey)
>
> 1. Verify that you have a supported version of OneNote.
>   See [OneNote Version Check](OneNote-Version-Check.md) on how to do that.
> 2. Make sure _OneNote_ is **not running**! Exit _OneNote_ if necessary.
> 3. Open a new PowerShell command prompt with **admin privileges**.
> 4. Excecute one of the following commands
>
>    **First Time Install**
>    ~~~ powershell
>    PS C:\> choco install onenote-taggingkit-addin.install
>    ~~~
>
>    **Upgrade**
>    ~~~ powershell
>    PS C:\> choco upgrade onenote-taggingkit-addin.install
>    ~~~

Upon successfull installation the _OneNote Tagging Kit_ actions should now be
available on the `Home` tab in
the _Page Tags_ command group (next to the built-in Tags command group)

![Home Tab](images/TaggingKitRibbon.png)

If you do not see this ribbon extension please refer to
[Tagging Kit Actions Not Shown in Ribbon](Troubleshooting/Tagging%20Kit%20Not%20Shown%20in%20Ribbon.md)

# Reporting Bugs and Requesting Features

If you think the _Tagging Kit_ is not working as it should, please check the [troubleshooting tips](Troubleshooting/Troubleshooting-Tips.md)
first.

# TaggingKit Development

The developer documentation can be found at [here](https://github.com/WetHat/OnenoteTaggingKit/blob/master/TaggingKitSandcastle/Documentation/Home.md).

Following tools are required to develop your own version of the _TaggingKit_:

* VisualStudio 2019 or later. See [Install Visual Studio](https://docs.microsoft.com/en-us/visualstudio/install/install-visual-studio)
* Windows Installer Toolset [WiX](http://wixtoolset.org/).
  Get the [recommended build](http://wixtoolset.org/releases/)
* Sandcastle Help File Builder (SHFB) for building help files with the Sandcastle tools.
  Get the latest release for GitHub [EWSoftware/SHFB ](https://github.com/EWSoftware/SHFB/releases)
* If you want to manage your clone of the _TaggingKit_ on GitHub it is also recommended to use the _Git_ distributed version control system: Get it from the
  the [Git - Downloads](https://git-scm.com/downloads) page.
