using System;
using System.Linq;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.Templates.ViewModel;
using Xunit;

namespace Grynwald.ChangeLog.Test.Templates.ViewModel
{
    /// <summary>
    /// Tests for <see cref="ChangeLogEntryGroupViewModel"/>
    /// </summary>
    public class ChangeLogEntryGroupViewModelTest : TestBase
    {
        private readonly ChangeLogConfiguration m_DefaultConfiguration = ChangeLogConfigurationLoader.GetDefaultConfiguration();

        [Fact]
        public void Configuration_must_not_be_null()
        {
            Assert.Throws<ArgumentNullException>(() => new ChangeLogEntryGroupViewModel(null!, "Some Title", Array.Empty<ChangeLogEntryViewModel>()));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        public void Title_must_not_be_null_or_whitespace(string title)
        {
            Assert.Throws<ArgumentException>(() => new ChangeLogEntryGroupViewModel(m_DefaultConfiguration, title, Array.Empty<ChangeLogEntryViewModel>()));
        }

        [Fact]
        public void Entires_must_not_be_null()
        {
            Assert.Throws<ArgumentNullException>(() => new ChangeLogEntryGroupViewModel(m_DefaultConfiguration, "Some title", null!));
        }

        [Fact]
        public void Entires_are_sorted_by_date()
        {
            // ARRANGE
            var entries = new[]
            {
                GetChangeLogEntry(summary: "Summary 1", date: DateTime.Now.AddDays(1).Date),
                GetChangeLogEntry(summary: "Summary 2", date: DateTime.Now.AddDays(-2).Date),
                GetChangeLogEntry(summary: "Summary 3", date: DateTime.Now.AddDays(3).Date),
                GetChangeLogEntry(summary: "Summary 4", date: DateTime.Now.AddDays(4).Date),
            };

            // ACT 
            var sut = new ChangeLogEntryGroupViewModel(m_DefaultConfiguration, "Title", entries.Select(x => new ChangeLogEntryViewModel(m_DefaultConfiguration, x)));

            // ASSERT
            Assert.Collection(sut.Entries,
                e => Assert.Equal("Summary 2", e.Title),
                e => Assert.Equal("Summary 1", e.Title),
                e => Assert.Equal("Summary 3", e.Title),
                e => Assert.Equal("Summary 4", e.Title)
            );
        }
    }
}
