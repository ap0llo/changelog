using System;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.ConventionalCommits;
using Grynwald.ChangeLog.Model;
using Grynwald.ChangeLog.Templates;
using Grynwald.ChangeLog.Templates.Default;
using Xunit;

namespace Grynwald.ChangeLog.Test.Templates.Default
{
    /// <summary>
    /// Tests for <see cref="DefaultTemplate"/>
    /// </summary>
    public class DefaultTemplateTest : TemplateTest
    {
        protected override ITemplate GetTemplateInstance(ChangeLogConfiguration configuration) => new DefaultTemplate(configuration);


        [Fact]
        public void ChangeLog_with_multiple_versions_is_converted_to_expected_Markdown_01()
        {
            // Changelog with two versions.
            // All versions are empty (no entries)

            var changeLog = new ApplicationChangeLog()
            {
                GetSingleVersionChangeLog("1.2.3"),
                GetSingleVersionChangeLog("4.5.6"),
            };

            Approve(changeLog);
        }
    }
}
