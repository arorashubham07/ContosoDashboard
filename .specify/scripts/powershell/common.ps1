#!/usr/bin/env pwsh
# Common PowerShell functions analogous to common.sh

function Get-RepoRoot {
    try {
        $result = git rev-parse --show-toplevel 2>$null
        if ($LASTEXITCODE -eq 0) {
            return $result
        }
    } catch {
        # Git command failed
    }
    
    # Fall back to script location for non-git repos
    return (Resolve-Path (Join-Path $PSScriptRoot "../../..")).Path
}

function Get-CurrentBranch {
    # First check if SPECIFY_FEATURE environment variable is set
    if ($env:SPECIFY_FEATURE) {
        return $env:SPECIFY_FEATURE
    }
    
    # Then check git if available
    try {
        $result = git rev-parse --abbrev-ref HEAD 2>$null
        if ($LASTEXITCODE -eq 0) {
            return $result
        }
    } catch {
        # Git command failed
    }
    
    # For non-git repos, try to find the latest feature directory
    $repoRoot = Get-RepoRoot
    $specsDir = Join-Path $repoRoot "specs"
    
    if (Test-Path $specsDir) {
        $latestFeature = ""
        $highest = 0
        
        Get-ChildItem -Path $specsDir -Directory | ForEach-Object {
            if ($_.Name -match '^(\d{3})-') {
                $num = [int]$matches[1]
                if ($num -gt $highest) {
                    $highest = $num
                    $latestFeature = $_.Name
                }
            }
        }
        
        if ($latestFeature) {
            return $latestFeature
        }
    }
    
    # Final fallback
    return "main"
}

function Test-HasGit {
    try {
        git rev-parse --show-toplevel 2>$null | Out-Null
        return ($LASTEXITCODE -eq 0)
    } catch {
        return $false
    }
}

function Test-FeatureBranch {
    param(
        [string]$Branch,
        [bool]$HasGit = $true
    )
    
    # For non-git repos, we can't enforce branch naming but still provide output
    if (-not $HasGit) {
        Write-Warning "[specify] Warning: Git repository not detected; skipped branch validation"
        return $true
    }
    
    if ($Branch -notmatch '^[0-9]{3}-') {
        Write-Output "ERROR: Not on a feature branch. Current branch: $Branch"
        Write-Output "Feature branches should be named like: 001-feature-name"
        return $false
    }
    return $true
}

function Get-FeatureDir {
    param([string]$RepoRoot, [string]$Branch)
    Join-Path $RepoRoot "specs/$Branch"
}

function Get-FeaturePathsEnv {
    $repoRoot = Get-RepoRoot
    $currentBranch = Get-CurrentBranch
    $hasGit = Test-HasGit
    $featureDir = Get-FeatureDir -RepoRoot $repoRoot -Branch $currentBranch
    
    [PSCustomObject]@{
        REPO_ROOT     = $repoRoot
        CURRENT_BRANCH = $currentBranch
        HAS_GIT       = $hasGit
        FEATURE_DIR   = $featureDir
        FEATURE_SPEC  = Join-Path $featureDir 'spec.md'
        IMPL_PLAN     = Join-Path $featureDir 'plan.md'
        TASKS         = Join-Path $featureDir 'tasks.md'
        RESEARCH      = Join-Path $featureDir 'research.md'
        DATA_MODEL    = Join-Path $featureDir 'data-model.md'
        QUICKSTART    = Join-Path $featureDir 'quickstart.md'
        CONTRACTS_DIR = Join-Path $featureDir 'contracts'
    }
}

function Get-TemplatePackageIds {
    param([string]$Directory, [string]$RegistryKey)

    $registryPath = Join-Path $Directory '.registry'
    if (Test-Path -LiteralPath $registryPath -PathType Leaf) {
        $registry = Get-Content -LiteralPath $registryPath -Raw | ConvertFrom-Json
        if ($null -eq $registry -or $registry -isnot [PSCustomObject]) {
            throw "Invalid template registry: $registryPath"
        }
        $packages = $registry.PSObject.Properties[$RegistryKey]
        if ($packages) {
            if ($packages.Value -isnot [PSCustomObject]) {
                throw "Invalid '$RegistryKey' mapping in template registry: $registryPath"
            }
            $packages.Value.PSObject.Properties |
                Where-Object {
                    $_.Name -cmatch '^[a-z0-9-]+$' -and
                    $_.Value -is [PSCustomObject] -and
                    (-not $_.Value.PSObject.Properties['enabled'] -or $_.Value.enabled)
                } |
                Sort-Object @{ Expression = {
                    $priority = 10
                    $parsedPriority = 0
                    if ([int]::TryParse([string]$_.Value.priority, [ref]$parsedPriority) -and $parsedPriority -gt 0) {
                        $priority = $parsedPriority
                    }
                    $priority
                } }, Name |
                ForEach-Object { $_.Name }
        }
        return
    }

    Get-ChildItem -LiteralPath $Directory -Directory |
        Where-Object { $_.Name -cmatch '^[a-z0-9-]+$' } |
        Sort-Object Name |
        ForEach-Object { $_.Name }
}

function Resolve-Template {
    param(
        [Parameter(Mandatory=$true)][string]$TemplateName,
        [Parameter(Mandatory=$true)][string]$RepoRoot
    )

    if ($TemplateName -cnotmatch '^[a-z0-9-]+$') { return $null }

    $templatesDir = Join-Path $RepoRoot '.specify/templates'
    $override = Join-Path $templatesDir "overrides/$TemplateName.md"
    if (Test-Path -LiteralPath $override -PathType Leaf) { return $override }

    foreach ($packageType in @('presets', 'extensions')) {
        $packageDir = Join-Path $RepoRoot ".specify/$packageType"
        if (-not (Test-Path -LiteralPath $packageDir -PathType Container)) { continue }
        foreach ($packageId in Get-TemplatePackageIds -Directory $packageDir -RegistryKey $packageType) {
            foreach ($relativePath in @("templates/$TemplateName.md", "$TemplateName.md")) {
                $candidate = Join-Path $packageDir "$packageId/$relativePath"
                if (Test-Path -LiteralPath $candidate -PathType Leaf) { return $candidate }
            }
        }
    }

    $core = Join-Path $templatesDir "$TemplateName.md"
    if (Test-Path -LiteralPath $core -PathType Leaf) { return $core }
    return $null
}

function Resolve-TemplateContent {
    param(
        [Parameter(Mandatory=$true)][string]$TemplateName,
        [Parameter(Mandatory=$true)][string]$RepoRoot
    )

    $template = Resolve-Template -TemplateName $TemplateName -RepoRoot $RepoRoot
    if ($null -eq $template) { return $null }
    return [System.IO.File]::ReadAllText($template, [System.Text.Encoding]::UTF8)
}

function Test-FileExists {
    param([string]$Path, [string]$Description)
    if (Test-Path -Path $Path -PathType Leaf) {
        Write-Output "  ✓ $Description"
        return $true
    } else {
        Write-Output "  ✗ $Description"
        return $false
    }
}

function Test-DirHasFiles {
    param([string]$Path, [string]$Description)
    if ((Test-Path -Path $Path -PathType Container) -and (Get-ChildItem -Path $Path -ErrorAction SilentlyContinue | Where-Object { -not $_.PSIsContainer } | Select-Object -First 1)) {
        Write-Output "  ✓ $Description"
        return $true
    } else {
        Write-Output "  ✗ $Description"
        return $false
    }
}

