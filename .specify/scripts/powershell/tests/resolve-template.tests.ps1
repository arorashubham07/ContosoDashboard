$ErrorActionPreference = 'Stop'
. "$PSScriptRoot/../common.ps1"

function Assert-Equal {
    param($Actual, $Expected, [string]$Description)
    if ($Actual -cne $Expected) {
        throw "FAIL: $Description. Expected '$Expected', got '$Actual'."
    }
    Write-Output "PASS: $Description"
}

function Set-Fixture {
    param([string]$RelativePath, [string]$Content)
    $path = Join-Path $fixtureRoot $RelativePath
    [System.IO.Directory]::CreateDirectory((Split-Path $path -Parent)) | Out-Null
    [System.IO.File]::WriteAllText($path, $Content, [System.Text.Encoding]::UTF8)
}

$fixtureRoot = Join-Path ([System.IO.Path]::GetTempPath()) "speckit-resolver-$([guid]::NewGuid())"
try {
    $templateName = 'constitution-template'
    Assert-Equal (Resolve-TemplateContent $templateName $fixtureRoot) $null 'Missing template returns null'
    Assert-Equal (Resolve-TemplateContent '../constitution-template' $fixtureRoot) $null 'Path traversal is rejected'

    $coreContent = "# Constitution`r`nUnicode: $([char]0x00E9)`r`n"
    Set-Fixture '.specify/templates/constitution-template.md' $coreContent
    Assert-Equal (Resolve-TemplateContent $templateName $fixtureRoot) $coreContent 'Core content preserves UTF-8 and line endings'

    Set-Fixture '.specify/extensions/sample/templates/constitution-template.md' 'extension'
    Assert-Equal (Resolve-TemplateContent $templateName $fixtureRoot) 'extension' 'Extension wins over core'

    Set-Fixture '.specify/presets/alpha/templates/constitution-template.md' 'alpha'
    Set-Fixture '.specify/presets/beta/constitution-template.md' 'beta'
    Assert-Equal (Resolve-TemplateContent $templateName $fixtureRoot) 'alpha' 'Presets win over extensions in alphabetical fallback order'

    Set-Fixture '.specify/presets/.registry' (@{
        presets = @{
            alpha = @{ priority = 20 }
            beta = @{ priority = 1 }
        }
    } | ConvertTo-Json -Depth 4)
    Assert-Equal (Resolve-TemplateContent $templateName $fixtureRoot) 'beta' 'Registry priority and flat package paths are honored'

    Set-Fixture '.specify/presets/.registry' (@{
        presets = @{
            alpha = @{ enabled = $false }
            beta = @{ enabled = $false }
        }
    } | ConvertTo-Json -Depth 4)
    Assert-Equal (Resolve-TemplateContent $templateName $fixtureRoot) 'extension' 'Disabled presets are skipped'

    Set-Fixture '.specify/extensions/.registry' (@{
        extensions = @{ sample = @{ enabled = $false } }
    } | ConvertTo-Json -Depth 4)
    Assert-Equal (Resolve-TemplateContent $templateName $fixtureRoot) $coreContent 'Disabled extensions are skipped'

    Set-Fixture '.specify/templates/overrides/constitution-template.md' 'override'
    Assert-Equal (Resolve-TemplateContent $templateName $fixtureRoot) 'override' 'Project override wins over all other layers'
    Set-Fixture '.specify/templates/overrides/constitution-template.md' ''
    Assert-Equal (Resolve-TemplateContent $templateName $fixtureRoot) '' 'Empty override does not fall through to core'

    $repoRoot = Get-RepoRoot
    $json = & "$PSScriptRoot/../resolve-template.ps1" constitution-template -Json | ConvertFrom-Json
    Assert-Equal $json.TEMPLATE_NAME $templateName 'CLI returns the requested template name'
    Assert-Equal $json.TEMPLATE_CONTENT (Resolve-TemplateContent $templateName $repoRoot) 'CLI JSON returns resolved content'

    $shell = (Get-Process -Id $PID).Path
    $process = Start-Process -FilePath $shell -ArgumentList @(
        '-NoProfile', '-File', "`"$PSScriptRoot/../resolve-template.ps1`"", 'missing-test-template', '-Json'
    ) -Wait -PassThru -NoNewWindow
    Assert-Equal $process.ExitCode 1 'CLI exits with code 1 for a missing template'
} finally {
    if (Test-Path -LiteralPath $fixtureRoot) {
        Remove-Item -LiteralPath $fixtureRoot -Recurse -Force
    }
}