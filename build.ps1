$ErrorActionPreference = "Stop"

if ($env:TF_BUILD) {
    Write-Host "##[group]Install .NET SDK"
}

# Install .NET 6 and 7 runtimes (requried for running tests on this platform)
./build/dotnet-install.ps1 -Channel 6.0 -Runtime dotnet
./build/dotnet-install.ps1 -Channel 7.0 -Runtime dotnet

# Install SDK and runtime as specified in global.json
./build/dotnet-install.ps1 -JsonFile "$PSScriptRoot/global.json"

Invoke-Expression "dotnet --info"

if ($env:TF_BUILD) {
    Write-Host "##[endgroup]"
}

dotnet run --project build/Build.csproj -- $args
exit $LASTEXITCODE