$ErrorActionPreference = "Stop"

# Install .NET Core 3.1 and .NET 5 runtimes (requried for running tests on these platforms)
./build/dotnet-install.ps1 -Channel 3.1 -Runtime dotnet
./build/dotnet-install.ps1 -Channel 5.0 -Runtime dotnet

# Install SDK and runtime as specified in global.json
./build/dotnet-install.ps1 -JsonFile "$PSScriptRoot/global.json"

dotnet run --project build/Build.csproj -- $args
exit $LASTEXITCODE