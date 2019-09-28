<#
.SYNOPSIS
Build a chocolatey deployment package (*.nupgk) from a template.

.DESCRIPTION
This script is designed to run after the application installer has been successfully built.
Typically the script is run in the post-build-event.

It is assumed that current directory when the script is started is the
installer output directory (`bin\Debug` or `bin\Release`).


The chocolate package is built from a chocolatey template directory.
The PowerShell (*.ps1) and spec files (*.nusoev) can have following placeholders:
* __version__ - the long form of the application version. A dotted quad of the form
  `major.minor.build.patch`
' __versionshort__ - Short form of the version string: `major.minor`

All files are copied to a chocolatey package directory with the same name of
the template directory (with all placeholders expanded) and a chocolatey
package (*.nupkg) is generated which can be uploaded to chocolatey.org
or hosted in a private chocolatey directory.

.PARAMETER dll
Fully qualified path to a dll whose file version is the same as the
product version of the application

.PARAMETER template
Fully qualified path to the chocolatey package specification directory.

.INPUTS
None. This script does not read from a pipe.

#>

param (
    [parameter(Mandatory=$true,ValueFromPipeline=$false)]
    [ValidateNotNullOrEmpty()]
    [string]$dll,

    [parameter(Mandatory=$true,ValueFromPipeline=$false)]
    [ValidateNotNullOrEmpty()]
    [string]$template
)

# get the version of the application to be packaged with chocolatey
$version = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($dll).FileVersion
$versionshort = $version -replace '\.\d+\.\d+$',''

# determine the name of the chocolatey project dir
$chocodir = Split-Path $template -Leaf

if (Test-Path -LiteralPath $chocodir) {
    # remove that directory
    Remove-Item $chocodir -Recurse
}

$basepath = $template.Length

Get-ChildItem -LiteralPath $template -Recurse `
| ForEach-Object {
    $relativePath = $_.FullName.Substring($basepath)
    $targetPath = Join-Path -Path "$pwd/$chocodir" -ChildPath $relativePath
    if ($_ -is [System.IO.DirectoryInfo]) {
        New-Item -Path $targetPath -ItemType Directory
    } elseif ($_.Extension -in ".ps1", ".nuspec") {
        # expand the version templates
        Get-Content -LiteralPath $_.FullName -Encoding UTF8 | ForEach-Object {
            $expanded = $_ -replace '__version__',$version
            $expanded -replace '__versionshort__',$versionshort
        } | Out-File -LiteralPath $targetPath -Encoding utf8
    } else {
        Copy-Item -LiteralPath $_.FullName -Destination $targetPath
    }
}

echo "Building chocolatey package in '$pwd/$chocodir'"
cd $chocodir
choco pack

