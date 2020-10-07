using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Nuke.Common;
using Nuke.Common.CI.AzurePipelines;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Coverlet;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.ReportGenerator;
using Nuke.Common.Utilities.Collections;
using static DotNetFormatTasks;
using static DotNetTasks;
using static NbgvTasks;
using static Nuke.CodeGeneration.CodeGenerator;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Logger;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.ReportGenerator.ReportGeneratorTasks;

//TODO: Pass /warnaserror to msbuild / dotnet.exe

[CheckBuildProjectConfigurations]
[UnsetVisualStudioEnvironmentVariables]
class BuildProcess : NukeBuild
{
    public static int Main() => Execute<BuildProcess>(x => x.Build);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("During test execution, collect code coverage")]
    readonly bool CollectCoverage = false;

    [Parameter("Skip execution of the '" + nameof(Build) + "' target")]
    readonly bool NoBuild = false;

    [Parameter("Skip execution of the '" + nameof(Restore) + "' target")]
    readonly bool NoRestore = false;


    [Solution] readonly Solution Solution;

    [GitRepository] readonly GitRepository GitRepository;

    AbsolutePath SourceDirectory => RootDirectory / "src";

    AbsolutePath BuildDirectory => RootDirectory / "build";

    AbsolutePath RootOutputDirectory => Host == HostType.AzurePipelines
        ? (AbsolutePath)AzurePipelines.Instance.BinariesDirectory
        : (RootDirectory / "Binaries");

    AbsolutePath TestResultsDirectory => RootOutputDirectory / "TestResults";

    AbsolutePath CodeCoverageReportDirectory => RootOutputDirectory / "TestResults" / "Coverage";

    AbsolutePath PackageOutputDirectory => RootOutputDirectory / Configuration / "packages";

    bool IsAzurePipelinesBuild => Host == HostType.AzurePipelines;


    Target Clean => _ => _
        .Description("Delete all build outputs and intermediate files")
        .Before(Restore)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/obj").ForEach(DeleteDirectory);
            DeleteDirectory(RootOutputDirectory);
        });

    Target Restore => _ => _
        .Description("Restore all NuGet dependencies")
        .OnlyWhenDynamic(() => !(NoBuild || NoRestore))
        .Executes(() =>
        {
            Info("Restoring NuGet packages");
            DotNetRestore(_ => _
                .SetProjectFile(Solution)
            );

            Info("Restoring .NET local tools");
            RootDirectory
                .GlobFiles("**/dotnet-tools.json")
                .ForEach(manifest =>
                    DotNetToolRestore(_ => _
                        .SetToolManifest(manifest)
                ));
        });

    Target Build => _ => _
        .Description("Build the repository")
        .OnlyWhenDynamic(() => !NoBuild)
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .EnableNoRestore()
            );
        });

    Target SetBuildNumber => _ => _
        .Unlisted()
        .DependsOn(Restore)
        .TriggeredBy(Build)
        .OnlyWhenDynamic(() => IsServerBuild)
        .Executes(() =>
        {
            NbgvCloud(_ => _.EnableAllVariables());

            if (IsAzurePipelinesBuild)
            {
                var versionInfo = Nbgv.GetVersion();
                var json = JsonConvert.SerializeObject(versionInfo.CloudBuildVariables, Formatting.Indented);

                var outFilePath = ((AbsolutePath)AzurePipelines.Instance.ArtifactStagingDirectory) / "Variables" / "nbgv.json";
                Directory.CreateDirectory(outFilePath.Parent);
                File.WriteAllText(outFilePath, json);

                AzurePipelines.Instance.UploadArtifacts("", "Variables", outFilePath);
            }
        });

    Target Test => _ => _
        .Description("Run all tests and optionally collect code coverage")
        .DependsOn(Build, CleanTestResults)
        .Executes(() =>
        {
            // Run tests
            DotNetTest(_ => _
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .EnableNoBuild()
                .SetResultsDirectory(TestResultsDirectory)
                .When(CollectCoverage, _ => _
                    .SetDataCollector("XPlat Code Coverage")
                    .SetCoverletOutputFormat(CoverletOutputFormat.cobertura)
            ));

            if (CollectCoverage)
            {
                var coverageReports = TestResultsDirectory.GlobFiles("**/*.cobertura.xml").Select(x => (string)x);

                // Print result files
                coverageReports.ForEach(result => Info($"Coverage result file: {result}"));

                // Generate coverage report
                ReportGenerator(_ => _
                    .SetFramework("netcoreapp3.0")
                    .SetReports(coverageReports)
                    .SetTargetDirectory(CodeCoverageReportDirectory)
                    .SetHistoryDirectory(CodeCoverageReportDirectory / "History")
                );

                CodeCoverageReportDirectory
                    .GlobFiles("index.html")
                    .NotEmpty("Code coverage report not found")
                    .ForEach(x => Success($"Coverage Report: {x}"));
            }
        });

    Target Pack => _ => _
        .Description("Create NuGet Packages")
        .DependsOn(Build)
        .Executes(() =>
        {
            PackageOutputDirectory
                .GlobFiles("*.nupkg")
                .ForEach(DeleteFile);

            DotNetPack(_ => _
                .SetProject(Solution)
                .SetConfiguration(Configuration)
                .EnableNoBuild()
            );

            PackageOutputDirectory
                .GlobFiles("*.nupkg")
                .NotEmpty("No packages found in package output directory")
                .ForEach(package =>
                {
                    Success($"Created package {package}");

                    if (IsAzurePipelinesBuild)
                        AzurePipelines.Instance.UploadArtifacts("", "Binaries", package);
                });
        });

    Target CleanTestResults => _ => _
        .Unlisted()
        .Executes(() =>
        {
            TestResultsDirectory.GlobDirectories("*")
                .Except(new[] { CodeCoverageReportDirectory })
                .ForEach(DeleteDirectory);
        });

    Target Generate => _ => _
        .Description("Update auto-generated files")
        .Executes(() =>
        {
            var specificationDirectory = BuildDirectory / "specifications";
            Info($"Specification directory: {specificationDirectory}");
            GenerateCodeFromDirectory(specificationDirectory);
        });

    Target FormatCode => _ => _
        .Description("Apply formatting rules to all code files")
        .DependsOn(Restore)
        .Executes(() =>
        {
            Info($"Formatting code in '{SourceDirectory}'");
            DotNetFormat(_ => _
                .EnableFolderMode()
                .SetProject(SourceDirectory)
            );

            Info($"Formatting code in '{BuildDirectory}'");
            DotNetFormat(_ => _
                .EnableFolderMode()
                .SetProject(BuildDirectory)
            );
        });
}
