using Nuke.Common;
using Nuke.Common.CI.AzurePipelines;

partial class BuildProcess
{
    enum ArtifactType
    {
        Binaries,
        TestResults,
        Variables,
        ChangeLog
    }


    void PublishArtifact(ArtifactType type, string path)
    {
        if (IsAzurePipelinesBuild)
        {
            AzurePipelines.Instance.UploadArtifacts("", type.ToString(), path);
        }
    }
}
