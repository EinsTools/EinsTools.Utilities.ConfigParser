#! /usr/local/bin/pwsh

param(
    [Parameter(Mandatory = $false)]
    [string] $NUGETTOKEN = ""
)

$ErrorActionPreference = "Stop"

function Invoke-Git {
    param(
        [Parameter(Mandatory = $true, ValueFromRemainingArguments = $true)]
        [string[]]$arguments
    )
    Write-Host "dotnet $arguments"
    & git @arguments
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet exited with code $LASTEXITCODE"
    }
}

function Invoke-DotNet {
    param(
        [Parameter(Mandatory = $true, ValueFromRemainingArguments = $true)]
        [string[]]$arguments
    )
    Write-Host "dotnet $arguments"
    & dotnet @arguments
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet exited with code $LASTEXITCODE"
    }
}

$ErrorActionPreference = "Stop"
Invoke-Git push --tags

# Read the content of the build.json file
$buildJson = Get-Content -Raw -Path build.json | ConvertFrom-Json
$pushBranches = [string[]]$buildJson.pushBranches
Write-Host "Pushing to github"

# Get the name of the current branch
$Branch = Invoke-Git rev-parse --abbrev-ref HEAD
if ($Branch -ne "HEAD")
{
    Invoke-DotNet nuget push ./publish/ConfigParser/*.nupkg --source github --skip-duplicate

    if ($Branch -in $pushBranches)
    {
        Write-Host "Pushing to nuget"
        if (![string]::IsNullOrEmpty($NUGETTOKEN))
        {
            Invoke-DotNet nuget push ./publish/ConfigParser/*.nupkg --source nuget --api-key $NUGETTOKEN
        }
    }
}