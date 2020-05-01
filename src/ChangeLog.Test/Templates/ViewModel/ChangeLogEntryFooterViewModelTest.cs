using System;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.ConventionalCommits;
using Grynwald.ChangeLog.Model;
using Grynwald.ChangeLog.Templates.ViewModel;
using Xunit;

namespace Grynwald.ChangeLog.Test.Templates.ViewModel
{
    /// <summary>
    /// Tests for <see cref="ChangeLogEntryFooterViewModel"/>
    /// </summary>
    public class ChangeLogEntryFooterViewModelTest
    {
        private readonly ChangeLogConfiguration m_DefaultConfiguration = ChangeLogConfigurationLoader.GetDefaultConfiguration();

        [Fact]
        public void Configuration_must_not_be_null()
        {
            Assert.Throws<ArgumentNullException>(() => new ChangeLogEntryFooterViewModel(null!, new ChangeLogEntryFooter(new CommitMessageFooterName("some-name"), "some-value")));
            Assert.Throws<ArgumentNullException>(() => new ChangeLogEntryFooterViewModel(null!, "some-name", "some value", null));
        }

        [Fact]
        public void Model_must_not_be_null()
        {
            Assert.Throws<ArgumentNullException>(() => new ChangeLogEntryFooterViewModel(m_DefaultConfiguration, null!));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        public void Name_must_not_be_null_or_whitespace(string name)
        {
            Assert.Throws<ArgumentException>(() => new ChangeLogEntryFooterViewModel(m_DefaultConfiguration, name, "some value", null));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        public void Value_must_not_be_null_or_whitespace(string value)
        {
            Assert.Throws<ArgumentException>(() => new ChangeLogEntryFooterViewModel(m_DefaultConfiguration, "someName", value, null));
        }


        [Fact]
        public void Name_retuns_display_name_if_configured_01()
        {
            // ARRANGE

            var config = new ChangeLogConfiguration()
            {
                Footers = new[]
                {
                    new ChangeLogConfiguration.FooterConfiguration() { Name = "some-footer", DisplayName = "Some Display Name"}
                }
            };

            // ACT 
            var sut = new ChangeLogEntryFooterViewModel(config, "some-footer", "some value", null);

            // ASSERT
            Assert.Equal("Some Display Name", sut.DisplayName);
        }

        [Fact]
        public void Name_retuns_display_name_if_configured_02()
        {
            // ARRANGE

            var config = new ChangeLogConfiguration()
            {
                Footers = new[]
                {
                    new ChangeLogConfiguration.FooterConfiguration() { Name = "some-footer", DisplayName = "Some Display Name"}
                }
            };

            // ACT
            var model = new ChangeLogEntryFooter(new CommitMessageFooterName("some-footer"), "some value");
            var sut = new ChangeLogEntryFooterViewModel(config, model);

            // ASSERT
            Assert.Equal("Some Display Name", sut.DisplayName);
        }
    }
}
