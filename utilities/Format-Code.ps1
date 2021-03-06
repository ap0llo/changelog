#
# This scripts runs the "dotnet-format" tool over the entire code base and fixes formatting errors.
# The script schould be run before every commit.
# The CI pipeline in Azure DevOps verifies the formatting and the build will fail if any formatting errors are found
#

# Imports
. (Join-Path $PSScriptRoot "common.ps1")

# Variables
$sourceFolders = @("./src", "./utilities") # The path of the folder to format (relative to the repository root)

# Main script
Push-Location (Get-RepositoryRoot)
try {
    log "Restoring tools"
    exec "dotnet tool restore"

    log "Running dotnet format"
    foreach($folder in $sourceFolders) {
        exec "dotnet format `"$folder`" --folder"
    }
}
finally {
    Pop-Location
}
