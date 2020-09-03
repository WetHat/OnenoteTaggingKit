$packageName   = 'onenote-taggingkit-addin.install'
$url           = 'https://github.com/WetHat/OnenoteTaggingKit/releases/download/v3.7/SetupTaggingKitWiX3.7.7551.20257.msi'
$silentArgs    = '/qn' 
$validExitCodes = @(0) 

Install-ChocolateyPackage -packageName   $packageName `
                          -FileType      'MSI'        `
                          -SilentArgs     $silentArgs `
                          -Url            $url `
                          -Checksum       '15fc524814f772113c7d625b569fdc502afb533a0f1791eae363d66bb67a02e7' `
                          -ChecksumType   'sha256' `
                          -validExitCodes $validExitCodes
