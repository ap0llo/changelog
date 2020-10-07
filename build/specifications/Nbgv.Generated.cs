
using JetBrains.Annotations;
using Newtonsoft.Json;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.Tooling;
using Nuke.Common.Tools;
using Nuke.Common.Utilities.Collections;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;

/// <summary>
///   <p>For more details, visit the <a href="https://github.com/dotnet/nerdbank.gitversioning">official website</a>.</p>
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public static partial class NbgvTasks
{
    /// <summary>
    ///   Path to the Nbgv executable.
    /// </summary>
    public static string NbgvPath =>
        ToolPathResolver.TryGetEnvironmentExecutable("NBGV_EXE") ??
        ToolPathResolver.GetPathExecutable("dotnet");
    public static Action<OutputType, string> NbgvLogger { get; set; } = ProcessTasks.DefaultLogger;
    /// <summary>
    ///   <p>For more details, visit the <a href="https://github.com/dotnet/nerdbank.gitversioning">official website</a>.</p>
    /// </summary>
    public static IReadOnlyCollection<Output> Nbgv(string arguments, string workingDirectory = null, IReadOnlyDictionary<string, string> environmentVariables = null, int? timeout = null, bool? logOutput = null, bool? logInvocation = null, Func<string, string> outputFilter = null)
    {
        var process = ProcessTasks.StartProcess(NbgvPath, arguments, workingDirectory, environmentVariables, timeout, logOutput, logInvocation, NbgvLogger, outputFilter);
        process.AssertZeroExitCode();
        return process.Output;
    }
    /// <summary>
    ///   <p>For more details, visit the <a href="https://github.com/dotnet/nerdbank.gitversioning">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>--format</c> via <see cref="NbgvGetVersionSettings.Format"/></li>
    ///   </ul>
    /// </remarks>
    public static IReadOnlyCollection<Output> NbgvGetVersion(NbgvGetVersionSettings toolSettings = null)
    {
        toolSettings = toolSettings ?? new NbgvGetVersionSettings();
        var process = ProcessTasks.StartProcess(toolSettings);
        process.AssertZeroExitCode();
        return process.Output;
    }
    /// <summary>
    ///   <p>For more details, visit the <a href="https://github.com/dotnet/nerdbank.gitversioning">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>--format</c> via <see cref="NbgvGetVersionSettings.Format"/></li>
    ///   </ul>
    /// </remarks>
    public static IReadOnlyCollection<Output> NbgvGetVersion(Configure<NbgvGetVersionSettings> configurator)
    {
        return NbgvGetVersion(configurator(new NbgvGetVersionSettings()));
    }
    /// <summary>
    ///   <p>For more details, visit the <a href="https://github.com/dotnet/nerdbank.gitversioning">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>--format</c> via <see cref="NbgvGetVersionSettings.Format"/></li>
    ///   </ul>
    /// </remarks>
    public static IEnumerable<(NbgvGetVersionSettings Settings, IReadOnlyCollection<Output> Output)> NbgvGetVersion(CombinatorialConfigure<NbgvGetVersionSettings> configurator, int degreeOfParallelism = 1, bool completeOnFailure = false)
    {
        return configurator.Invoke(NbgvGetVersion, NbgvLogger, degreeOfParallelism, completeOnFailure);
    }
    /// <summary>
    ///   <p>For more details, visit the <a href="https://github.com/dotnet/nerdbank.gitversioning">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>--all-vars</c> via <see cref="NbgvCloudSettings.AllVariables"/></li>
    ///     <li><c>--ci-system</c> via <see cref="NbgvCloudSettings.CiSystem"/></li>
    ///   </ul>
    /// </remarks>
    public static IReadOnlyCollection<Output> NbgvCloud(NbgvCloudSettings toolSettings = null)
    {
        toolSettings = toolSettings ?? new NbgvCloudSettings();
        var process = ProcessTasks.StartProcess(toolSettings);
        process.AssertZeroExitCode();
        return process.Output;
    }
    /// <summary>
    ///   <p>For more details, visit the <a href="https://github.com/dotnet/nerdbank.gitversioning">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>--all-vars</c> via <see cref="NbgvCloudSettings.AllVariables"/></li>
    ///     <li><c>--ci-system</c> via <see cref="NbgvCloudSettings.CiSystem"/></li>
    ///   </ul>
    /// </remarks>
    public static IReadOnlyCollection<Output> NbgvCloud(Configure<NbgvCloudSettings> configurator)
    {
        return NbgvCloud(configurator(new NbgvCloudSettings()));
    }
    /// <summary>
    ///   <p>For more details, visit the <a href="https://github.com/dotnet/nerdbank.gitversioning">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>--all-vars</c> via <see cref="NbgvCloudSettings.AllVariables"/></li>
    ///     <li><c>--ci-system</c> via <see cref="NbgvCloudSettings.CiSystem"/></li>
    ///   </ul>
    /// </remarks>
    public static IEnumerable<(NbgvCloudSettings Settings, IReadOnlyCollection<Output> Output)> NbgvCloud(CombinatorialConfigure<NbgvCloudSettings> configurator, int degreeOfParallelism = 1, bool completeOnFailure = false)
    {
        return configurator.Invoke(NbgvCloud, NbgvLogger, degreeOfParallelism, completeOnFailure);
    }
}
#region NbgvGetVersionSettings
/// <summary>
///   Used within <see cref="NbgvTasks"/>.
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
[Serializable]
public partial class NbgvGetVersionSettings : ToolSettings
{
    /// <summary>
    ///   Path to the Nbgv executable.
    /// </summary>
    public override string ToolPath => base.ToolPath ?? NbgvTasks.NbgvPath;
    public override Action<OutputType, string> CustomLogger => NbgvTasks.NbgvLogger;
    public virtual NbgvOutputFormat Format { get; internal set; } = NbgvOutputFormat.text;
    protected override Arguments ConfigureArguments(Arguments arguments)
    {
        arguments
          .Add("tool run nbgv -- get-version")
          .Add("--format {value}", Format);
        return base.ConfigureArguments(arguments);
    }
}
#endregion
#region NbgvCloudSettings
/// <summary>
///   Used within <see cref="NbgvTasks"/>.
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
[Serializable]
public partial class NbgvCloudSettings : ToolSettings
{
    /// <summary>
    ///   Path to the Nbgv executable.
    /// </summary>
    public override string ToolPath => base.ToolPath ?? NbgvTasks.NbgvPath;
    public override Action<OutputType, string> CustomLogger => NbgvTasks.NbgvLogger;
    public virtual bool? AllVariables { get; internal set; }
    public virtual NbgvCiSystem CiSystem { get; internal set; }
    protected override Arguments ConfigureArguments(Arguments arguments)
    {
        arguments
          .Add("tool run nbgv -- cloud")
          .Add("--all-vars", AllVariables)
          .Add("--ci-system {value}", CiSystem);
        return base.ConfigureArguments(arguments);
    }
}
#endregion
#region NbgvGetVersionSettingsExtensions
/// <summary>
///   Used within <see cref="NbgvTasks"/>.
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public static partial class NbgvGetVersionSettingsExtensions
{
    #region Format
    /// <summary>
    ///   <p><em>Sets <see cref="NbgvGetVersionSettings.Format"/></em></p>
    /// </summary>
    [Pure]
    public static T SetFormat<T>(this T toolSettings, NbgvOutputFormat format) where T : NbgvGetVersionSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Format = format;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="NbgvGetVersionSettings.Format"/></em></p>
    /// </summary>
    [Pure]
    public static T ResetFormat<T>(this T toolSettings) where T : NbgvGetVersionSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Format = null;
        return toolSettings;
    }
    #endregion
}
#endregion
#region NbgvCloudSettingsExtensions
/// <summary>
///   Used within <see cref="NbgvTasks"/>.
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public static partial class NbgvCloudSettingsExtensions
{
    #region AllVariables
    /// <summary>
    ///   <p><em>Sets <see cref="NbgvCloudSettings.AllVariables"/></em></p>
    /// </summary>
    [Pure]
    public static T SetAllVariables<T>(this T toolSettings, bool? allVariables) where T : NbgvCloudSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.AllVariables = allVariables;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="NbgvCloudSettings.AllVariables"/></em></p>
    /// </summary>
    [Pure]
    public static T ResetAllVariables<T>(this T toolSettings) where T : NbgvCloudSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.AllVariables = null;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Enables <see cref="NbgvCloudSettings.AllVariables"/></em></p>
    /// </summary>
    [Pure]
    public static T EnableAllVariables<T>(this T toolSettings) where T : NbgvCloudSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.AllVariables = true;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Disables <see cref="NbgvCloudSettings.AllVariables"/></em></p>
    /// </summary>
    [Pure]
    public static T DisableAllVariables<T>(this T toolSettings) where T : NbgvCloudSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.AllVariables = false;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Toggles <see cref="NbgvCloudSettings.AllVariables"/></em></p>
    /// </summary>
    [Pure]
    public static T ToggleAllVariables<T>(this T toolSettings) where T : NbgvCloudSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.AllVariables = !toolSettings.AllVariables;
        return toolSettings;
    }
    #endregion
    #region CiSystem
    /// <summary>
    ///   <p><em>Sets <see cref="NbgvCloudSettings.CiSystem"/></em></p>
    /// </summary>
    [Pure]
    public static T SetCiSystem<T>(this T toolSettings, NbgvCiSystem ciSystem) where T : NbgvCloudSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.CiSystem = ciSystem;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="NbgvCloudSettings.CiSystem"/></em></p>
    /// </summary>
    [Pure]
    public static T ResetCiSystem<T>(this T toolSettings) where T : NbgvCloudSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.CiSystem = null;
        return toolSettings;
    }
    #endregion
}
#endregion
#region NbgvOutputFormat
/// <summary>
///   Used within <see cref="NbgvTasks"/>.
/// </summary>
[PublicAPI]
[Serializable]
[ExcludeFromCodeCoverage]
[TypeConverter(typeof(TypeConverter<NbgvOutputFormat>))]
public partial class NbgvOutputFormat : Enumeration
{
    public static NbgvOutputFormat text = (NbgvOutputFormat) "text";
    public static NbgvOutputFormat json = (NbgvOutputFormat) "json";
    public static explicit operator NbgvOutputFormat(string value)
    {
        return new NbgvOutputFormat { Value = value };
    }
}
#endregion
#region NbgvCiSystem
/// <summary>
///   Used within <see cref="NbgvTasks"/>.
/// </summary>
[PublicAPI]
[Serializable]
[ExcludeFromCodeCoverage]
[TypeConverter(typeof(TypeConverter<NbgvCiSystem>))]
public partial class NbgvCiSystem : Enumeration
{
    public static NbgvCiSystem AppVeyor = (NbgvCiSystem) "AppVeyor";
    public static NbgvCiSystem VisualStudioTeamServices = (NbgvCiSystem) "VisualStudioTeamServices";
    public static NbgvCiSystem GitHubActions = (NbgvCiSystem) "GitHubActions";
    public static NbgvCiSystem TeamCity = (NbgvCiSystem) "TeamCity";
    public static NbgvCiSystem AtlassianBamboo = (NbgvCiSystem) "AtlassianBamboo";
    public static NbgvCiSystem Jenkins = (NbgvCiSystem) "Jenkins";
    public static NbgvCiSystem GitLab = (NbgvCiSystem) "GitLab";
    public static NbgvCiSystem Travis = (NbgvCiSystem) "Travis";
    public static explicit operator NbgvCiSystem(string value)
    {
        return new NbgvCiSystem { Value = value };
    }
}
#endregion
