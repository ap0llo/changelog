using ApprovalTests;
using ApprovalTests.Reporters;
using ChangeLogCreator.Model;
using ChangeLogCreator.Tasks;
using Xunit;

namespace ChangeLogCreator.Test.Tasks
{
    [UseReporter(typeof(DiffReporter))]
    public class RenderMarkdownTaskTest : TestBase
    {
        [Fact]
        public void ChangeLog_is_converted_to_expected_Markdown_01()
        {
            Approve(new ChangeLog());
        }

        [Fact]
        public void ChangeLog_is_converted_to_expected_Markdown_02()
        {
            var changeLog = new ChangeLog()
            {
                GetSingleVersionChangeLog("1.2.3"),
                GetSingleVersionChangeLog("4.5.6"),
            };
            Approve(changeLog);
        }

        [Fact]
        public void ChangeLog_is_converted_to_expected_Markdown_03()
        {
            var versionChangeLog = GetSingleVersionChangeLog("1.2.3");

            var dates = new DateTimeSource();

            versionChangeLog.Add(GetChangeLogEntry(type: "feat", summary: "Some change", date: dates.Next()));
            versionChangeLog.Add(GetChangeLogEntry(type: "fix", summary: "A bug was fixed", date: dates.Next()));
            versionChangeLog.Add(GetChangeLogEntry(type: "feat", summary: "Some other change", date: dates.Next()));

            var changeLog = new ChangeLog()
            {
                versionChangeLog
            };

            Approve(changeLog);
        }

        [Fact]
        public void ChangeLog_is_converted_to_expected_Markdown_04()
        {
            var versionChangeLog = GetSingleVersionChangeLog("1.2.3");

            var dates = new DateTimeSource();

            versionChangeLog.Add(GetChangeLogEntry(scope: "api", type: "feat", summary: "Some change", date: dates.Next()));
            versionChangeLog.Add(GetChangeLogEntry(scope: "cli", type: "fix", summary: "A bug was fixed", date: dates.Next()));
            versionChangeLog.Add(GetChangeLogEntry(scope: "", type: "feat", summary: "Some other change", date: dates.Next()));

            var changeLog = new ChangeLog()
            {
                versionChangeLog
            };

            Approve(changeLog);
        }


        private void Approve(ChangeLog changeLog)
        {
            var sut = new RenderMarkdownTask("DummyPath");

            var doc = sut.GetChangeLogDocument(changeLog);

            Assert.NotNull(doc);

            var markdown = doc.ToString();

            var writer = new ApprovalTextWriter(markdown);
            Approvals.Verify(writer, new ApprovalNamer(), Approvals.GetReporter());
        }
    }
}
