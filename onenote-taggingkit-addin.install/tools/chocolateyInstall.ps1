$packageName   = 'onenote-taggingkit-addin.install'
$url           = 'https://github.com/WetHat/OnenoteTaggingKit/releases/download/v3.6/SetupTaggingKitWiX.3.6.7210.24294.msi'
$silentArgs    = '/qn' 
$validExitCodes = @(0) 

Install-ChocolateyPackage -packageName   $packageName `
                          -FileType      'MSI'        `
                          -SilentArgs     $silentArgs `
                          -Url            $url `
                          -Checksum       'd5c45390524da8fbae2075e2885a4965abcd210b1c3e55fec0edd7683a4de375' `
                          -ChecksumType   'sha256' `
                          -validExitCodes $validExitCodes
