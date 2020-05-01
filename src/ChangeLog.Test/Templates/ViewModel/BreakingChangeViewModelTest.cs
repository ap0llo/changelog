using System;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.Templates.ViewModel;
using Xunit;

namespace Grynwald.ChangeLog.Test.Templates.ViewModel
{
    /// <summary>
    /// Tests for <see cref="BreakingChangeViewModel"/>
    /// </summary>
    public class BreakingChangeViewModelTest : TestBase
    {
        private readonly ChangeLogConfiguration m_DefaultConfiguration = ChangeLogConfigurationLoader.GetDefaultConfiguration();


        [Fact]
        public void Configuration_must_not_be_null()
        {
            Assert.Throws<ArgumentNullException>(() => new BreakingChangeViewModel(null!, "description", new ChangeLogEntryViewModel(m_DefaultConfiguration, GetChangeLogEntry()), false));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        public void Description_must_not_be_null_or_whitespace(string description)
        {
            Assert.Throws<ArgumentException>(() => new BreakingChangeViewModel(m_DefaultConfiguration, description, new ChangeLogEntryViewModel(m_DefaultConfiguration, GetChangeLogEntry()), false));
        }

        [Fact]
        public void Entry_must_not_be_null()
        {
            Assert.Throws<ArgumentNullException>(() => new BreakingChangeViewModel(m_DefaultConfiguration, "Some description", null!, false));
        }
    }
}
