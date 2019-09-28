$packageName   = 'onenote-taggingkit-addin.install'
$url           = 'https://github.com/WetHat/OnenoteTaggingKit/releases/download/v3.5/SetupTaggingKitWiX.3.5.6972.33231.msi'
$silentArgs    = '/qn' 
$validExitCodes = @(0) 

Install-ChocolateyPackage -packageName   $packageName `
                          -FileType      'MSI'        `
                          -SilentArgs     $silentArgs `
                          -Url            $url `
                          -Checksum       '0C6C968DF3118DAC2C796A21DBD66009E4B758C6D6F06BF20D1E3BF166DBE270' `
                          -ChecksumType   'sha256' `
                          -validExitCodes $validExitCodes