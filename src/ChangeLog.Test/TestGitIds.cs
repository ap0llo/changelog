using Grynwald.ChangeLog.Git;

namespace Grynwald.ChangeLog.Test
{
    /// <summary>
    /// Provides test <see cref="GitId"/> values
    /// </summary>
    public class TestGitIds
    {
        public static readonly GitId Id1 = new GitId("abcd1234abcd1234abcd1234abcd1234abcd1234", "abcd123");
        public static readonly GitId Id2 = new GitId("efgh5678efgh5678efgh5678efgh5678efgh5678", "efgh567");
        public static readonly GitId Id3 = new GitId("ijkl9101ijkl9101ijkl9101ijkl9101ijkl9101", "ijkl910");
        public static readonly GitId Id4 = new GitId("mnop1234mnop1234mnop1234mnop1234mnop1234", "mnop123");
    }
}
