$packageName   = 'onenote-taggingkit-addin.install'
$url           = 'https://github.com/WetHat/OnenoteTaggingKit/releases/download/v3.8.7617/SetupTaggingKitWiX.3.8.7617.36152.msi'
$silentArgs    = '/qn' 
$validExitCodes = @(0,1603) 

Install-ChocolateyPackage -packageName   $packageName `
                          -FileType      'MSI'        `
                          -SilentArgs     $silentArgs `
                          -Url            $url `
                          -Checksum       '2a1a0ab90227b80d49b92131f048f65e22df5c92b2f46a5f579cdd1c30478fe6' `
                          -ChecksumType   'sha256' `
                          -validExitCodes $validExitCodes
