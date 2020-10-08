
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
///   <p>For more details, visit the <a href="https://github.com/dotnet/format">official website</a>.</p>
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public static partial class DotNetFormatTasks
{
    /// <summary>
    ///   Path to the DotNetFormat executable.
    /// </summary>
    public static string DotNetFormatPath =>
        ToolPathResolver.TryGetEnvironmentExecutable("DOTNETFORMAT_EXE") ??
        ToolPathResolver.GetPathExecutable("dotnet");
    public static Action<OutputType, string> DotNetFormatLogger { get; set; } = ProcessTasks.DefaultLogger;
    /// <summary>
    ///   <p>For more details, visit the <a href="https://github.com/dotnet/format">official website</a>.</p>
    /// </summary>
    public static IReadOnlyCollection<Output> DotNetFormat(string arguments, string workingDirectory = null, IReadOnlyDictionary<string, string> environmentVariables = null, int? timeout = null, bool? logOutput = null, bool? logInvocation = null, Func<string, string> outputFilter = null)
    {
        var process = ProcessTasks.StartProcess(DotNetFormatPath, arguments, workingDirectory, environmentVariables, timeout, logOutput, logInvocation, DotNetFormatLogger, outputFilter);
        process.AssertZeroExitCode();
        return process.Output;
    }
    /// <summary>
    ///   <p>For more details, visit the <a href="https://github.com/dotnet/format">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>&lt;project&gt;</c> via <see cref="DotNetFormatSettings.Project"/></li>
    ///     <li><c>--check</c> via <see cref="DotNetFormatSettings.CheckMode"/></li>
    ///     <li><c>--folder</c> via <see cref="DotNetFormatSettings.FolderMode"/></li>
    ///     <li><c>--report</c> via <see cref="DotNetFormatSettings.ReportPath"/></li>
    ///   </ul>
    /// </remarks>
    public static IReadOnlyCollection<Output> DotNetFormat(DotNetFormatSettings toolSettings = null)
    {
        toolSettings = toolSettings ?? new DotNetFormatSettings();
        var process = ProcessTasks.StartProcess(toolSettings);
        process.AssertZeroExitCode();
        return process.Output;
    }
    /// <summary>
    ///   <p>For more details, visit the <a href="https://github.com/dotnet/format">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>&lt;project&gt;</c> via <see cref="DotNetFormatSettings.Project"/></li>
    ///     <li><c>--check</c> via <see cref="DotNetFormatSettings.CheckMode"/></li>
    ///     <li><c>--folder</c> via <see cref="DotNetFormatSettings.FolderMode"/></li>
    ///     <li><c>--report</c> via <see cref="DotNetFormatSettings.ReportPath"/></li>
    ///   </ul>
    /// </remarks>
    public static IReadOnlyCollection<Output> DotNetFormat(Configure<DotNetFormatSettings> configurator)
    {
        return DotNetFormat(configurator(new DotNetFormatSettings()));
    }
    /// <summary>
    ///   <p>For more details, visit the <a href="https://github.com/dotnet/format">official website</a>.</p>
    /// </summary>
    /// <remarks>
    ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
    ///   <ul>
    ///     <li><c>&lt;project&gt;</c> via <see cref="DotNetFormatSettings.Project"/></li>
    ///     <li><c>--check</c> via <see cref="DotNetFormatSettings.CheckMode"/></li>
    ///     <li><c>--folder</c> via <see cref="DotNetFormatSettings.FolderMode"/></li>
    ///     <li><c>--report</c> via <see cref="DotNetFormatSettings.ReportPath"/></li>
    ///   </ul>
    /// </remarks>
    public static IEnumerable<(DotNetFormatSettings Settings, IReadOnlyCollection<Output> Output)> DotNetFormat(CombinatorialConfigure<DotNetFormatSettings> configurator, int degreeOfParallelism = 1, bool completeOnFailure = false)
    {
        return configurator.Invoke(DotNetFormat, DotNetFormatLogger, degreeOfParallelism, completeOnFailure);
    }
}
#region DotNetFormatSettings
/// <summary>
///   Used within <see cref="DotNetFormatTasks"/>.
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
[Serializable]
public partial class DotNetFormatSettings : ToolSettings
{
    /// <summary>
    ///   Path to the DotNetFormat executable.
    /// </summary>
    public override string ToolPath => base.ToolPath ?? DotNetFormatTasks.DotNetFormatPath;
    public override Action<OutputType, string> CustomLogger => DotNetFormatTasks.DotNetFormatLogger;
    public virtual string Project { get; internal set; } = ".";
    public virtual bool? FolderMode { get; internal set; }
    public virtual bool? CheckMode { get; internal set; }
    public virtual string ReportPath { get; internal set; }
    protected override Arguments ConfigureArguments(Arguments arguments)
    {
        arguments
          .Add("format")
          .Add("{value}", Project)
          .Add("--folder", FolderMode)
          .Add("--check", CheckMode)
          .Add("--report {value}", ReportPath);
        return base.ConfigureArguments(arguments);
    }
}
#endregion
#region DotNetFormatSettingsExtensions
/// <summary>
///   Used within <see cref="DotNetFormatTasks"/>.
/// </summary>
[PublicAPI]
[ExcludeFromCodeCoverage]
public static partial class DotNetFormatSettingsExtensions
{
    #region Project
    /// <summary>
    ///   <p><em>Sets <see cref="DotNetFormatSettings.Project"/></em></p>
    /// </summary>
    [Pure]
    public static T SetProject<T>(this T toolSettings, string project) where T : DotNetFormatSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Project = project;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="DotNetFormatSettings.Project"/></em></p>
    /// </summary>
    [Pure]
    public static T ResetProject<T>(this T toolSettings) where T : DotNetFormatSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.Project = null;
        return toolSettings;
    }
    #endregion
    #region FolderMode
    /// <summary>
    ///   <p><em>Sets <see cref="DotNetFormatSettings.FolderMode"/></em></p>
    /// </summary>
    [Pure]
    public static T SetFolderMode<T>(this T toolSettings, bool? folderMode) where T : DotNetFormatSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.FolderMode = folderMode;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="DotNetFormatSettings.FolderMode"/></em></p>
    /// </summary>
    [Pure]
    public static T ResetFolderMode<T>(this T toolSettings) where T : DotNetFormatSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.FolderMode = null;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Enables <see cref="DotNetFormatSettings.FolderMode"/></em></p>
    /// </summary>
    [Pure]
    public static T EnableFolderMode<T>(this T toolSettings) where T : DotNetFormatSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.FolderMode = true;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Disables <see cref="DotNetFormatSettings.FolderMode"/></em></p>
    /// </summary>
    [Pure]
    public static T DisableFolderMode<T>(this T toolSettings) where T : DotNetFormatSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.FolderMode = false;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Toggles <see cref="DotNetFormatSettings.FolderMode"/></em></p>
    /// </summary>
    [Pure]
    public static T ToggleFolderMode<T>(this T toolSettings) where T : DotNetFormatSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.FolderMode = !toolSettings.FolderMode;
        return toolSettings;
    }
    #endregion
    #region CheckMode
    /// <summary>
    ///   <p><em>Sets <see cref="DotNetFormatSettings.CheckMode"/></em></p>
    /// </summary>
    [Pure]
    public static T SetCheckMode<T>(this T toolSettings, bool? checkMode) where T : DotNetFormatSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.CheckMode = checkMode;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="DotNetFormatSettings.CheckMode"/></em></p>
    /// </summary>
    [Pure]
    public static T ResetCheckMode<T>(this T toolSettings) where T : DotNetFormatSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.CheckMode = null;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Enables <see cref="DotNetFormatSettings.CheckMode"/></em></p>
    /// </summary>
    [Pure]
    public static T EnableCheckMode<T>(this T toolSettings) where T : DotNetFormatSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.CheckMode = true;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Disables <see cref="DotNetFormatSettings.CheckMode"/></em></p>
    /// </summary>
    [Pure]
    public static T DisableCheckMode<T>(this T toolSettings) where T : DotNetFormatSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.CheckMode = false;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Toggles <see cref="DotNetFormatSettings.CheckMode"/></em></p>
    /// </summary>
    [Pure]
    public static T ToggleCheckMode<T>(this T toolSettings) where T : DotNetFormatSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.CheckMode = !toolSettings.CheckMode;
        return toolSettings;
    }
    #endregion
    #region ReportPath
    /// <summary>
    ///   <p><em>Sets <see cref="DotNetFormatSettings.ReportPath"/></em></p>
    /// </summary>
    [Pure]
    public static T SetReportPath<T>(this T toolSettings, string reportPath) where T : DotNetFormatSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.ReportPath = reportPath;
        return toolSettings;
    }
    /// <summary>
    ///   <p><em>Resets <see cref="DotNetFormatSettings.ReportPath"/></em></p>
    /// </summary>
    [Pure]
    public static T ResetReportPath<T>(this T toolSettings) where T : DotNetFormatSettings
    {
        toolSettings = toolSettings.NewInstance();
        toolSettings.ReportPath = null;
        return toolSettings;
    }
    #endregion
}
#endregion
