﻿using System;
using System.IO;
using CommandLine;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.Templates;
using Grynwald.Utilities.Configuration;

namespace Grynwald.ChangeLog.CommandLine
{
    /// <summary>
    /// Defines the command line parameters for the "generate" sub-command
    /// </summary>
    [Verb(CommandNames.Generate, isDefault: true, aliases: new string[] { "g" }, HelpText = "Generate a change log from a git repository")]
    public class GenerateCommandLineParameters
    {
        private string? m_OutputPath;


        [Option('r', "repository", Required = false, HelpText = "The local path of the git repository to generate a change log for.")]
        public string RepositoryPath { get; set; } = "";

        [Option('c', "configurationFilePath", Required = false, HelpText =
            "The path of the configuration file to use. " +
            "When no configuration file path is specified, changelog looks for a file named 'changelog.settings.json' in the repository directory. " +
            "If no configuration file is found, default settings are used.")]
        public string? ConfigurationFilePath { get; set; }

        [Option('o', "outputPath", Required = false, HelpText = "The path to save the changelog to.")]
        [ConfigurationValue("changelog:outputPath")]
        public string? OutputPath
        {
            // if value is set, return an absolute path
            // otherwise, if a relative path is passed through the configuration system
            // it would be interpreted as being relative to the repository path.
            // However, when specifying a relative path on the command line, it makes much more sense
            // for that path to be relative to the current directory
            get => String.IsNullOrEmpty(m_OutputPath) ? null : Path.GetFullPath(m_OutputPath);
            set => m_OutputPath = value;
        }

        [Option("githubAccessToken", Required = false, HelpText = "The access token to use if the GitHub integration is enabled.")]
        [ConfigurationValue("changelog:integrations:github:accesstoken")]
        public string? GitHubAccessToken { get; set; }

        [Option("gitlabAccessToken", Required = false, HelpText = "The access token to use if the GitLab integration is enabled.")]
        [ConfigurationValue("changelog:integrations:gitlab:accesstoken")]
        public string? GitLabAccessToken { get; set; }

        [Option('v', "verbose", Required = false, Default = false, HelpText = "Increase the level of detail for messages logged to the console.")]
        public bool Verbose { get; set; }

        [Option("versionRange", Required = false, Default = null, HelpText = "The range of versions to include in the change log. Value must be a valid NuGet version range.")]
        [ConfigurationValue("changelog:versionrange")]
        public string? VersionRange { get; set; }

        [Option("currentVersion", Required = false, Default = null, HelpText = "Sets the version of the currently checked-out commit. Value must be a valid semantic version")]
        [ConfigurationValue("changelog:currentVersion")]
        public string? CurrentVersion { get; set; }

        [Option("template", Required = false, Default = null, HelpText = "Sets the template to use for generating the changelog.")]
        [ConfigurationValue("changelog:template:name")]
        public TemplateName? Template { get; set; }

        [Option("integrationProvider", Required = false, Default = null, HelpText = "Sets the integration provider to use")]
        [ConfigurationValue("changelog:integrations:provider")]
        public string? IntegrationProvider { get; set; }
    }
}
