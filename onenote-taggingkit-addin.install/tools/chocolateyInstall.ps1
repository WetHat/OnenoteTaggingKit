$packageName   = 'onenote-taggingkit-addin.install'
$url           = 'https://github.com/WetHat/OnenoteTaggingKit/releases/download/v3.7.7581/SetupTaggingKitWiX.3.7.7581.19466.msi'
$silentArgs    = '/qn' 
$validExitCodes = @(0) 

Install-ChocolateyPackage -packageName   $packageName `
                          -FileType      'MSI'        `
                          -SilentArgs     $silentArgs `
                          -Url            $url `
                          -Checksum       '51223d2e3fc66456fd79b0d9c10981c4f9abcd27506bb4a87ebaa9f001ccced6' `
                          -ChecksumType   'sha256' `
                          -validExitCodes $validExitCodes
