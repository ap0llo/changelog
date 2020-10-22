# Imports
. (Join-Path $PSScriptRoot "common.ps1")


# Main script
Push-Location (Get-RepositoryRoot)
try {

    exec "dotnet run --project ./utilities/docs/docs.csproj -- generate ./docs"
}
finally {
    Pop-Location
}
