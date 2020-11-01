<#
.SYNOPSIS
    Updates auto-generated files (documentation files and JSON schema) in this repository
#>

. (Join-Path $PSScriptRoot "common.ps1")

Push-Location (Get-RepositoryRoot)
try {

    exec "dotnet run --project ./utilities/docs/docs.csproj -- generate ./docs"
    exec "dotnet run --project ./utilities/schema/schema.csproj -- generate ./schemas/configuration/schema.json"
}
finally {
    Pop-Location
}
