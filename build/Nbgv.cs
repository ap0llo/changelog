using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Nuke.Common.Tooling;
using static NbgvTasks;

class NbgvVersionInfo
{
    [JsonRequired, JsonProperty("CloudBuildNumber")]
    public string CloudBuildNumber { get; set; }

    [JsonRequired, JsonProperty("NuGetPackageVersion")]
    public string NuGetPackageVersion { get; set; }

    [JsonRequired, JsonProperty("CloudBuildAllVars")]
    public IDictionary<string, string> CloudBuildVariables { get; set; } = new Dictionary<string, string>();
}

class Nbgv
{
    public static NbgvVersionInfo GetVersion()
    {
        var output = NbgvGetVersion(_ => _
                .SetFormat(NbgvOutputFormat.json)
                .DisableLogOutput());

        var json = String.Join(Environment.NewLine, output.Where(x => x.Type == OutputType.Std).Select(x => x.Text));

        var versionInfo = JsonConvert.DeserializeObject<NbgvVersionInfo>(json);
        return versionInfo;
    }
}
