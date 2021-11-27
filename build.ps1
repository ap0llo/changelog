$ErrorActionPreference = "Stop"
./build/dotnet-install.ps1 -Channel 3.1 -Runtime dotnet
./build/dotnet-install.ps1 -Channel 5.0 -Runtime dotnet
./build/dotnet-install.ps1 -JsonFile ./global.json
dotnet run --project build/Build.csproj -- $args
exit $LASTEXITCODE;