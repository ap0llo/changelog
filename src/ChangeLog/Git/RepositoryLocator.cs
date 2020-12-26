using System;
using System.Diagnostics.CodeAnalysis;
using LibGit2Sharp;

namespace Grynwald.ChangeLog.Git
{
    /// <summary>
    /// Helper class to find to locate git repositories
    /// </summary>
    public sealed class RepositoryLocator
    {
        /// <summary>
        /// Attempts to get working directory of a git repository stating from <paramref name="startingPath"/>.
        /// </summary>
        /// <remarks>
        /// Searches <paramref name="startingPath"/> and any of its parent directories and checks if the directory is a valid git repository.
        /// </remarks>
        /// <param name="startingPath">The directory to start the search for a git repository.</param>
        /// <param name="repositoryPath">On success, contains the root path of the git repository</param>
        /// <returns>Returns <c>true</c> if a git repository was found, otherwise returns <c>false</c>.</returns>
        public static bool TryGetRepositoryPath(string startingPath, [NotNullWhen(true)] out string? repositoryPath)
        {
            var path = Repository.Discover(startingPath);

            if (!String.IsNullOrEmpty(path))
            {
                using var repo = new Repository(path);
                var workingDirectory = repo.Info.WorkingDirectory;

                if (!String.IsNullOrEmpty(workingDirectory))
                {
                    repositoryPath = workingDirectory;
                    return true;
                }
            }

            repositoryPath = default;
            return false;
        }
    }
}
