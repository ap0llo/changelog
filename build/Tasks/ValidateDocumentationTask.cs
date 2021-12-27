using Cake.Common.Tools.DotNetCore;
using Cake.Common.Tools.DotNetCore.Run;
using Cake.Core;
using Cake.Core.IO;
using Cake.Frosting;
using Grynwald.SharedBuild.Tasks;

namespace Build
{
    [TaskName("ValidateDocumentation")]
    [TaskDescription("Validates documentation files")]
    [Dependency(typeof(BuildTask))]
    [IsDependeeOf(typeof(ValidateTask))]
    public class ValidateDocumentationTask : FrostingTask<BuildContext>
    {
        public override void Run(BuildContext context)
        {
            context.DotNetCoreRun(
                "./utilities/docs/docs.csproj",
                new ProcessArgumentBuilder()
                    .Append("validate")
                    .Append("./docs")
                    .Append("README.md"),
                new DotNetCoreRunSettings()
                {
                    Configuration = context.BuildSettings.Configuration,
                    NoBuild = true,
                    NoRestore = true,
                }
            );

        }
    }
}
