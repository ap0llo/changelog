using System;
using System.Collections.Generic;
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

//TODO: Pass /warnaserror to msbuild/dotnet.exe

[CheckBuildProjectConfigurations]
[UnsetVisualStudioEnvironmentVariables]
class BuildProcess : NukeBuild
{
    public static int Main() => Execute<BuildProcess>(x => x.Build);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("During test execution, collect code coverage")]
    readonly bool CollectCoverage = IsServerBuild;

    [Parameter("Skip execution of the '" + nameof(Build) + "' target")]
    readonly bool NoBuild = false;

    [Parameter("Skip execution of the '" + nameof(Restore) + "' target")]
    readonly bool NoRestore = false;

    [Solution] readonly Solution Solution = null!;

    [GitRepository] readonly GitRepository GitRepository = null!;


    AbsolutePath SourceDirectory => RootDirectory / "src";

    AbsolutePath BuildDirectory => RootDirectory / "build";

    AbsolutePath RootOutputDirectory => Host == HostType.AzurePipelines
        ? (AbsolutePath)AzurePipelines.Instance.BinariesDirectory
        : (RootDirectory / "Binaries");


    AbsolutePath TestOutputDirectory => RootOutputDirectory / "TestResults" / "out";

    enum CoverageOutputDirectory
    {
        Html,
        Cobertura,
        History
    }

    AbsolutePath CodeCoverageReportDirectory(CoverageOutputDirectory dir) => RootOutputDirectory / "TestResults" / "Coverage" / dir.ToString();

    IEnumerable<string> CodeCoverageOutputFiles => TestOutputDirectory.GlobFiles("**/*.cobertura.xml")
        // When the trx logger and coverage collection is enabled, dotnet test saves the coverage output to two locations:
        // - <RESULTS-DIRECTORY>/<GUID>/coverage.cobertura.xml
        // - <TEST-RUN-NAME>/In/<COMPUTERNAME>/coverage.cobertura.xml
        // To avoid loading the same coverage files twice, ignore the second copy of the output file
        .Except(TestOutputDirectory.GlobFiles("**/In/**/*.cobertura.xml"))
        .Select(x => (string)x);

    IEnumerable<string> TestOutputFiles => TestOutputDirectory.GlobFiles("**/*.trx").Select(x => (string)x);


    AbsolutePath PackageOutputDirectory => RootOutputDirectory / Configuration / "packages";


    /// <summary>
    /// Gets all directories that contain code for which formatting rules should be applied to
    /// </summary>
    IEnumerable<AbsolutePath> CodeFormatDirectories
    {
        get
        {
            yield return SourceDirectory;
            yield return BuildDirectory;
        }
    }


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
            Info($"Configuration is {Configuration}");

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
                .SetResultsDirectory(TestOutputDirectory)
                .SetLogger("trx")
                .When(CollectCoverage, _ => _
                    .SetDataCollector("XPlat Code Coverage")
                    .SetCoverletOutputFormat(CoverletOutputFormat.cobertura)
            ));

            TestOutputFiles
                .NotEmpty()
                .ForEach(trxFile => Info($"Test result file: {trxFile}"));

            if (CollectCoverage)
            {
                // Print result files
                CodeCoverageOutputFiles
                    .NotEmpty()
                    .ForEach(result => Info($"Coverage result file: {result}"));

                // Generate coverage report
                ReportGenerator(_ => _
                    .SetFramework("netcoreapp3.0")
                    .SetReports(CodeCoverageOutputFiles)
                    .SetTargetDirectory(CodeCoverageReportDirectory(CoverageOutputDirectory.Html))
                    .SetHistoryDirectory(CodeCoverageReportDirectory(CoverageOutputDirectory.History))
                    .SetReportTypes(IsAzurePipelinesBuild ? ReportTypes.HtmlInline_AzurePipelines : ReportTypes.Html)
                );

                CodeCoverageReportDirectory(CoverageOutputDirectory.Html)
                    .GlobFiles("index.html")
                    .NotEmpty("Code coverage report not found")
                    .ForEach(x => Success($"Coverage Report: {x}"));
            }

        });

    Target PublishTestResults => _ => _
        .TriggeredBy(Test)
        .DependsOn(Restore)
        .After(Test)
        .Unlisted()
        .OnlyWhenStatic(() => IsAzurePipelinesBuild && CollectCoverage)
        .Executes(() =>
        {
            //
            // Publish test results
            //

            TestOutputFiles.ForEach(trxFile =>
            {
                var title = String.Join(", ",
                    TrxFile.GetAssemblyInfos(trxFile)
                           .Select(x => $"{x.Name} ({x.Framework?.ToString() ?? "Unknown Framework"})")
                );

                AzurePipelines.Instance.PublishTestResults(
                    title,
                    AzurePipelinesTestResultsType.VSTest,
                    new[] { trxFile }
                );

                AzurePipelines.Instance.UploadArtifacts("", "TestResults", trxFile);
            });

            //
            // Publish code coverage results
            //

            // The Azure Pipelines only supports publishing a single coverage result file,
            // so the results are merged into a single cobertura file using "ReportGenerator"
            // Generate coverage report
            ReportGenerator(_ => _
                .SetFramework("netcoreapp3.0")
                .SetReports(CodeCoverageOutputFiles)
                .SetTargetDirectory(CodeCoverageReportDirectory(CoverageOutputDirectory.Cobertura))
                .SetReportTypes(ReportTypes.Cobertura)
            );

            AzurePipelines.Instance.PublishCodeCoverage(
                    AzurePipelinesCodeCoverageToolType.Cobertura,
                    CodeCoverageReportDirectory(CoverageOutputDirectory.Cobertura).GlobFiles("*.xml").Single(),
                    CodeCoverageReportDirectory(CoverageOutputDirectory.Html)
            );
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
            DeleteDirectory(TestOutputDirectory);
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
            CodeFormatDirectories.ForEach(dir =>
            {
                Info($"Formatting code in '{dir}'");
                DotNetFormat(_ => _
                    .EnableFolderMode()
                    .SetProject(dir)
                );

            });
        });

    Target CheckCodeFormatting => _ => _
        .Description("Check if all code files follow the formatting rules")
        .DependsOn(Restore)
        .TriggeredBy(Test)
        .ProceedAfterFailure()
        .Executes(() =>
        {
            CodeFormatDirectories.ForEach(dir =>
            {
                Info($"Formatting code in '{dir}'");
                DotNetFormat(_ => _
                    .EnableFolderMode()
                    .SetProject(dir)
                    .EnableCheckMode()
                );
            });
        });
}
