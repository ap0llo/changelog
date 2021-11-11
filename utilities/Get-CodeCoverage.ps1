#
# This scripts runs "dotnet test" for the project, collects code coverage and gernated a coverage report
#

# Imports
. (Join-Path $PSScriptRoot "common.ps1")

# Variables
$testResultsDirectory = ".\Build\TestResults"
$coverageHistoryDirectory = ".\Build\CoverageHistory"

# Main script
Push-Location (Get-RepositoryRoot)
try {

    log "Restoring tools"
    exec "dotnet tool restore"

    if (Test-Path $testResultsDirectory) {
        log "Cleaning up test results directory"
        Remove-Item -Path $testResultsDirectory  -Recurse -Force
    }

    log "Running dotnet test with coverage"
    exec "dotnet test ./ChangeLog.sln --collect:`"XPlat Code Coverage`" "

    log "Generating code coverage report"
    exec "dotnet tool run reportgenerator -- `"-reports:$testResultsDirectory\*\coverage.cobertura.xml`" `"-targetdir:$testResultsDirectory\Coverage`" `"-reporttypes:html`" `"-historyDir:$coverageHistoryDirectory`" "

    $coverageReportPath = Join-Path (Get-Location) "$testResultsDirectory\Coverage\index.html"
    $coverageReportPath = [System.IO.Path]::GetFullPath($coverageReportPath)
    log "Code Coverage report generated to $coverageReportPath"
}
finally {
    Pop-Location
}
