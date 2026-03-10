param(
    [Parameter(Mandatory = $true)]
    [string]$InputFile,

    [Parameter(Mandatory = $true)]
    [string]$OutputFile,

    [string]$Image = "ghcr.io/mermaid-js/mermaid-cli/mermaid-cli:10.9.1",
    [string]$Theme = "default",
    [string]$BackgroundColor = "white",
    [int]$Width = 1800,
    [int]$Scale = 2,
    [switch]$ForceDocker
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Get-FullPath {
    param([string]$PathValue)

    if ([System.IO.Path]::IsPathRooted($PathValue)) {
        return [System.IO.Path]::GetFullPath($PathValue)
    }

    return [System.IO.Path]::GetFullPath((Join-Path (Get-Location) $PathValue))
}

function Invoke-MermaidViaNpx {
    param(
        [string]$ResolvedInputPath,
        [string]$ResolvedOutputPath,
        [string]$ResolvedPuppeteerConfigPath
    )

    if (-not (Get-Command npx -ErrorAction SilentlyContinue)) {
        return $false
    }

    $npxArgs = @(
        "-y",
        "@mermaid-js/mermaid-cli",
        "-p", $ResolvedPuppeteerConfigPath,
        "-i", $ResolvedInputPath,
        "-o", $ResolvedOutputPath,
        "-b", $BackgroundColor,
        "-t", $Theme,
        "-w", $Width.ToString(),
        "-s", $Scale.ToString()
    )

    & npx @npxArgs
    if ($LASTEXITCODE -eq 0 -and (Test-Path $ResolvedOutputPath)) {
        return $true
    }

    return $false
}

function Invoke-MermaidViaDocker {
    param(
        [string]$ResolvedInputPath,
        [string]$ResolvedOutputPath,
        [string]$ResolvedMermaidConfigPath
    )

    if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
        throw "Neither npx nor Docker was available for Mermaid rendering."
    }

    docker info | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw "Docker daemon is not running. Start Docker Desktop and retry."
    }

    $scratchRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("mermaid-render-" + [guid]::NewGuid().ToString("N"))
    $inputMount = Join-Path $scratchRoot "input"
    $outputMount = Join-Path $scratchRoot "output"

    New-Item -ItemType Directory -Path $inputMount -Force | Out-Null
    New-Item -ItemType Directory -Path $outputMount -Force | Out-Null

    $inputName = Split-Path -Leaf $ResolvedInputPath
    $outputName = Split-Path -Leaf $ResolvedOutputPath

    Copy-Item -Force $ResolvedInputPath (Join-Path $inputMount $inputName)

    try {
        $dockerArgs = @(
            "run",
            "--rm",
            "-v", "${inputMount}:/input",
            "-v", "${outputMount}:/output",
            "-v", "${PSScriptRoot}:/config",
            $Image,
            "-i", "/input/$inputName",
            "-o", "/output/$outputName",
            "-c", "/config/mermaid-config.json",
            "-t", $Theme,
            "-b", $BackgroundColor,
            "-w", $Width.ToString(),
            "-s", $Scale.ToString()
        )

        & docker @dockerArgs
        if ($LASTEXITCODE -ne 0) {
            throw "docker run failed with exit code $LASTEXITCODE"
        }

        $renderedPath = Join-Path $outputMount $outputName
        if (-not (Test-Path $renderedPath)) {
            throw "Container did not create the output file: $renderedPath"
        }

        Copy-Item -Force $renderedPath $ResolvedOutputPath
    }
    finally {
        if (Test-Path $scratchRoot) {
            Remove-Item -Recurse -Force $scratchRoot
        }
    }
}

$inputPath = Get-FullPath $InputFile
if (-not (Test-Path $inputPath)) {
    throw "Diagram source file was not found: $inputPath"
}

$outputPath = Get-FullPath $OutputFile
$outputDir = Split-Path -Parent $outputPath
if (-not (Test-Path $outputDir)) {
    New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
}

$mermaidConfigPath = Join-Path $PSScriptRoot "mermaid-config.json"
if (-not (Test-Path $mermaidConfigPath)) {
    throw "Mermaid config file was not found: $mermaidConfigPath"
}

$puppeteerConfigPath = Join-Path $PSScriptRoot "puppeteer-config.json"
if (-not (Test-Path $puppeteerConfigPath)) {
    throw "Puppeteer config file was not found: $puppeteerConfigPath"
}

if (-not $ForceDocker) {
    $renderedViaNpx = Invoke-MermaidViaNpx `
        -ResolvedInputPath $inputPath `
        -ResolvedOutputPath $outputPath `
        -ResolvedPuppeteerConfigPath $puppeteerConfigPath

    if ($renderedViaNpx) {
        Write-Output "written=$outputPath"
        return
    }
}

Invoke-MermaidViaDocker `
    -ResolvedInputPath $inputPath `
    -ResolvedOutputPath $outputPath `
    -ResolvedMermaidConfigPath $mermaidConfigPath

Write-Output "written=$outputPath"
