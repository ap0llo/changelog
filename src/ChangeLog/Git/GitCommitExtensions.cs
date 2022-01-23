namespace Grynwald.ChangeLog.Git
{
    internal static class GitCommitExtensions
    {
        // TODO: This can probably be replaced by making GitCommit into a record type, once we no longer target .NET Core 3.1
        public static GitCommit WithCommitMessage(this GitCommit commit, string commitMessage) =>
            new GitCommit(commit.Id, commitMessage, commit.Date, commit.Author);
    }
}
