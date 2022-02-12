using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Run;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Core.IO;
using Cake.Frosting;
using Cake.GitVersioning;
using Grynwald.SharedBuild;

namespace Build
{
    [TaskName(TaskNames.GenerateChangeLog)]
    [TaskDescription("Generates a change log for the current repository")]
    public class GenerateChangeLogTask : FrostingTask<IBuildContext>
    {
        public override void Run(IBuildContext context)
        {
            //
            // Generate change log
            //
            var versionOracle = context.GitVersioningGetVersion();

            var args = new ProcessArgumentBuilder()
                    .Append($"--currentVersion {versionOracle.NuGetPackageVersion}")
                    .Append($"--versionRange [{versionOracle.NuGetPackageVersion}]")
                    .Append($"--outputpath")
                    .Append(context.Output.ChangeLogFile.FullPath)
                    .Append($"--template GitHubRelease")
                    .Append($"--verbose");

            var dotnetCoreRunSettings = new DotNetRunSettings()
            {
                Configuration = context.BuildSettings.Configuration,
                NoBuild = true,
                NoRestore = true,
                Framework = "net6.0",
            };

            if (context.GitHub.TryGetAccessToken() is string accessToken)
            {
                context.Log.Information("GitHub access token specified, activating changelog's GitHub integration");
                args.Append("--integrationProvider");
                args.AppendQuoted("GitHub");
                dotnetCoreRunSettings.EnvironmentVariables["CHANGELOG__INTEGRATIONS__GITHUB__ACCESSTOKEN"] = accessToken;
            }
            else
            {
                context.Log.Warning("No GitHub access token specified, generating change log without GitHub integration");
            }

            context.DotNetRun(
                "./src/ChangeLog/Grynwald.ChangeLog.csproj",
                args,
                dotnetCoreRunSettings
            );

            //
            // Publish change log
            //
            if (context.AzurePipelines.IsActive)
            {
                context.AzurePipelines.Commands.UploadArtifact(
                    folderName: "",
                    file: context.Output.ChangeLogFile,
                    artifactName: context.AzurePipelines.ArtifactNames.ChangeLog
                );
            }
        }
    }
}
