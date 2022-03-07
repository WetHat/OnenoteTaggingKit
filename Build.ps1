<#
.SYNOPSIS
Build a static HTML site from Markdown files.
#>
[string]$SCRIPT:projectDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Import-Module -Name MarkDownToHTML

$OutputEncoding = [System.Text.Encoding]::UTF8 # Send utf8 to external programs

# Load JSON configuration
$SCRIPT:config = Get-Content (Join-Path $projectDir 'Build.json') | ConvertFrom-Json
if (!$config) {
    throw 'No build configuration found!'
}

# Determine the location of the static HTML site to build.
$SCRIPT:staticSite = Join-Path $projectDir $config.site_dir

# Clean up the static HTML site before build.
Remove-Item $staticSite -Recurse -Force -ErrorAction:SilentlyContinue

# Set-up the content mapping rules for replacing template placeholders
# of the form {{name}}.
$SCRIPT:contentMap = @{
	# Add additional mappings here...
	'{{footer}}' = {
		param($fragment)
		$fragment.EffectiveConfiguration.Footer # Fetch footer text from configuration
	}
	'{{nav}}'    = {
		param($fragment) # the annotated html fragment created from a markdown file
		$cfg = $fragment.EffectiveConfiguration
		$navcfg = $cfg.navigation_bar # navigation bar configuration
		# Create the navigation items configured in 'Build.json'
		New-SiteNavigation -NavitemSpecs $cfg.site_navigation `
		                   -RelativePath $fragment.RelativePath `
		                   -NavTemplate $navcfg.templates
		# Create navigation items to headings on the local page.
		# This requires the `autoidentifiers` extension to be enabled!
		New-PageHeadingNavigation -NavitemSpecs $cfg.page_navigation_header `
		                          -HTMLfragment $fragment `
		                          -NavTemplate $navcfg.templates `
		                          -HeadingLevels $navcfg.capture_page_headings
	}
}

# Conversion pipeline
$SCRIPT:markdown = Join-Path $projectDir $config.markdown_dir
Find-MarkdownFiles $markdown -Exclude $config.Exclude -BuildConfiguration $config `
| Convert-MarkdownToHTMLFragment -IncludeExtension $config.markdown_extensions -Split `
| Convert-SvgbobToSvg -SiteDirectory $staticSite `
| Publish-StaticHTMLSite -Template (Join-Path $projectDir $config.HTML_Template) `
                         -ContentMap  $contentMap `
						 -MediaDirectory $markdown `
	                     -SiteDirectory $staticSite

if ($config.github_pages) {
	# Switch off Jekyll publishing when building for GitHub pages
	New-Item -Path $staticSite -Name .nojekyll -ItemType File
}


