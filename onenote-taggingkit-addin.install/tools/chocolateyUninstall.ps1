$packageName = 'onenote-taggingkit-addin.install'
$namePattern = '*OneNote Tagging Kit*'

Get-ItemProperty -Path @( 'HKLM:\Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\*',
                          'HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\*' ) `
                 -ErrorAction:SilentlyContinue `
| Where-Object {
    $_.DisplayName -like $namePattern
  } `
| ForEach-Object {
    Uninstall-ChocolateyPackage $packageName 'MSI' "$($_.PSChildName) /qn /norestart"
  }