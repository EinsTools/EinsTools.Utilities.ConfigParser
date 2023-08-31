#! /usr/local/bin/pwsh

param(
    [Parameter(Mandatory = $false)]
    [string]$OutputPath = "./publish/ConfigParser"
)

$ErrorActionPreference = "Stop"

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

$project = "EinsTools.Utilities.ConfigParser/EinsTools.Utilities.ConfigParser.csproj"
Invoke-DotNet restore $project
Invoke-DotNet build -c Release --no-restore $project
Invoke-DotNet test -c Release --no-build --no-restore --verbosity normal $project
Invoke-DotNet pack -c Release --no-build --no-restore --verbosity normal $project --output $OutputPath

