using Cake.Common.Tools.DotNetCore;
using Cake.Common.Tools.DotNetCore.Run;
using Cake.Core;
using Cake.Core.IO;
using Cake.Frosting;
using Grynwald.SharedBuild.Tasks;

namespace Build
{
    [TaskName("ValidateConfigurationSchema")]
    [TaskDescription("Validates that the configuration file JSON schema is up to date")]
    [Dependency(typeof(BuildTask))]
    [IsDependeeOf(typeof(ValidateTask))]
    public class ValidateConfigurationSchemaTask : FrostingTask<BuildContext>
    {
        public override void Run(BuildContext context)
        {
            context.DotNetCoreRun(
                "./utilities/schema/schema.csproj",
                new ProcessArgumentBuilder()
                    .Append("validate")
                    .Append("./schemas/configuration/schema.json"),
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
