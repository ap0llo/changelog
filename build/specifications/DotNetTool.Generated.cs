
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
///   <p>For more details, visit the <a href="https://docs.microsoft.com/en-us/dotnet/core/tools/">official website</a>.</p>
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public static partial class DotNetTasks
{
    /// <summary>
    ///   Path to the DotNet executable.
    /// </summary>
    public static string DotNetPath =>
        ToolPathResolver.TryGetEnvironmentExecutable("DOTNET_EXE") ??
        ToolPathResolver.GetPathExecutable("dotnet");
    public static Action<OutputType, string> DotNetLogger { get; set; } = ProcessTasks.DefaultLogger;
    /// <summary>
    ///   <p>For more details, visit the <a href="https://docs.microsoft.com/en-us/dotnet/core/tools/">official website</a>.</p>
    /// </summary>
    public static IReadOnlyCollection<Output> DotNet(string arguments, string workingDirectory = null, IReadOnlyDictionary<string, string> environmentVariables = null, int? timeout = null, bool? logOutput = null, bool? logInvocation = null, Func<string, string> outputFilter = null)
    {
        var process = ProcessTasks.StartProcess(DotNetPath, arguments, workingDirectory, environmentVariables, timeout, logOutput, logInvocation, DotNetLogger, outputFilter);
        process.AssertZeroExitCode();
        return process.Output;
    }
    /// <summary>
    ///   <p>For more details, visit the <a href="https://docs.microsoft.com/en-us/dotnet/core/tools/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>--tool-manifest</c> via <see cref="DotNetToolRestoreSettings.ToolManifest"/></li>
    ///   </ul>
    /// </remarks>
    public static IReadOnlyCollection<Output> DotNetToolRestore(DotNetToolRestoreSettings toolSettings = null)
    {
        toolSettings = toolSettings ?? new DotNetToolRestoreSettings();
        var process = ProcessTasks.StartProcess(toolSettings);
        process.AssertZeroExitCode();
        return process.Output;
    }
    /// <summary>
    ///   <p>For more details, visit the <a href="https://docs.microsoft.com/en-us/dotnet/core/tools/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>--tool-manifest</c> via <see cref="DotNetToolRestoreSettings.ToolManifest"/></li>
    ///   </ul>
    /// </remarks>
    public static IReadOnlyCollection<Output> DotNetToolRestore(Configure<DotNetToolRestoreSettings> configurator)
    {
        return DotNetToolRestore(configurator(new DotNetToolRestoreSettings()));
    }
    /// <summary>
    ///   <p>For more details, visit the <a href="https://docs.microsoft.com/en-us/dotnet/core/tools/">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>--tool-manifest</c> via <see cref="DotNetToolRestoreSettings.ToolManifest"/></li>
    ///   </ul>
    /// </remarks>
    public static IEnumerable<(DotNetToolRestoreSettings Settings, IReadOnlyCollection<Output> Output)> DotNetToolRestore(CombinatorialConfigure<DotNetToolRestoreSettings> configurator, int degreeOfParallelism = 1, bool completeOnFailure = false)
    {
        return configurator.Invoke(DotNetToolRestore, DotNetLogger, degreeOfParallelism, completeOnFailure);
    }
}
#region DotNetToolRestoreSettings
/// <summary>
///   Used within <see cref="DotNetTasks"/>.
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
[Serializable]
public partial class DotNetToolRestoreSettings : ToolSettings
{
    /// <summary>
    ///   Path to the DotNet executable.
    /// </summary>
    public override string ToolPath => base.ToolPath ?? DotNetTasks.DotNetPath;
    public override Action<OutputType, string> CustomLogger => DotNetTasks.DotNetLogger;
    public virtual string ToolManifest { get; internal set; }
    protected override Arguments ConfigureArguments(Arguments arguments)
    {
        arguments
          .Add("tool restore")
          .Add("--tool-manifest {value}", ToolManifest);
        return base.ConfigureArguments(arguments);
    }
}
#endregion
#region DotNetToolRestoreSettingsExtensions
/// <summary>
///   Used within <see cref="DotNetTasks"/>.
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public static partial class DotNetToolRestoreSettingsExtensions
{
    #region ToolManifest
    /// <summary>
    ///   <p><em>Sets <see cref="DotNetToolRestoreSettings.ToolManifest"/></em></p>
    /// </summary>
    [Pure]
    public static T SetToolManifest<T>(this T toolSettings, string toolManifest) where T : DotNetToolRestoreSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.ToolManifest = toolManifest;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="DotNetToolRestoreSettings.ToolManifest"/></em></p>
    /// </summary>
    [Pure]
    public static T ResetToolManifest<T>(this T toolSettings) where T : DotNetToolRestoreSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.ToolManifest = null;
        return toolSettings;
    }
    #endregion
}
#endregion
