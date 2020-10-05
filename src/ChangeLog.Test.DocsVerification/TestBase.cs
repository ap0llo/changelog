using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LibGit2Sharp;

namespace Grynwald.ChangeLog.Test.DocsVerification
{
    public abstract class TestBase
    {
        protected static string RootPath
        {
            get
            {
                // When running in Azure Pipelines, the test output directory is located outside the source folder.
                // The 'Build.SourcesDirectory' variable defines where the sources are located
                var sourcesDirectory = Environment.GetEnvironmentVariable("BUILD_SOURCESDIRECTORY");
                if (!String.IsNullOrEmpty(sourcesDirectory))
                {
                    return sourcesDirectory;
                }

                // find root of repository (assumes the test output directory is a path in the repository)
                var gitPath = Repository.Discover(Environment.CurrentDirectory);

                // Repository.Discover() returns the path of the '.git' directory or file including a trailing backslash
                // => the repository root is the parent directory
                return Path.GetDirectoryName(gitPath.TrimEnd('\\')) ?? "";
            }
        }
    }
}
