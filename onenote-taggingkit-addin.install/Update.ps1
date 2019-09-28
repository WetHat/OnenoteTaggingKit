import-module au
$releases = 'https://github.com/WetHat/OnenoteTaggingKit/releases'

function global:au_SearchReplace {
    @{
       '.\tools\chocolateyInstall.ps1' = @{
            '(?i)(\s*\$url\s+=\s+'')[^'']+' = "`${1}$($Latest.URL32)"
            '(?i)(-Checksum\s+'')[^'']*'    = "`${1}$($Latest.Checksum32)"
        }
     }
}

function global:au_GetLatest {

    $downloadPage = Invoke-WebRequest -Uri $releases -UseBasicParsing

    $url32 = $downloadPage.links `
    | ForEach-Object { $_.href } `
    | Where-Object { $_ -like '*.msi' } `
    | Select-Object -First 1

    # the url looks like '/WetHat/OnenoteTaggingKit/releases/download/v3.6/SetupTaggingKitWiX.3.6.7210.24294.msi'
    $msi = $url32 -split '/' | Select-Object -Last 1

    $version = [regex]::Match($msi,'[\d.]+').value.trim('.')
    
    @{ URL32 = "https://github.com$url32"; Version = $version ; ChecksumType32 = 'sha256' }
}

update-package -ChecksumFor 32