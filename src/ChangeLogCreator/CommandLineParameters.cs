using System;
using System.IO;
using ChangeLogCreator.Configuration;
using CommandLine;

namespace ChangeLogCreator
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
            // is value is set, return an absolute path
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
    }
}
