using System;
using System.IO;
using Grynwald.ChangeLog.Configuration;
using CommandLine;

namespace Grynwald.ChangeLog
{
    public class CommandLineParameters
    {
        private string? m_OutputPath;

        [Option('r', "repository", Required = true)]
        [ConfigurationValue("changelog:repositoryPath")]
        public string RepositoryPath { get; set; } = "";

        [Option('o', "outputpath", Required = false)]
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

        [Option("gitHubAccessToken", Required = false)]
        [ConfigurationValue("changelog:integrations:github:accesstoken")]
        public string? GitHubAccessToken { get; set; }

        [Option("gitLabAccessToken", Required = false)]
        [ConfigurationValue("changelog:integrations:gitlab:accesstoken")]
        public string? GitLabAccessToken { get; set; }

        [Option("verbose", Required = false, Default = false)]
        public bool Verbose { get; set; }

        [Option("versionRange", Required = false, Default = null)]
        [ConfigurationValue("changelog:versionrange")]
        public string? VersionRange { get; set; }

        [Option("currentVersion", Required = false, Default = null, HelpText = "Sets the version of the currently checkout out commit. Value must be a valid semantic version")]
        [ConfigurationValue("changelog:currentVersion")]
        public string? CurrentVersion { get; set; }
    }
}
