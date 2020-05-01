using System;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.ConventionalCommits;
using Grynwald.ChangeLog.Model;
using Grynwald.ChangeLog.Templates.ViewModel;
using Xunit;

namespace Grynwald.ChangeLog.Test.Templates.ViewModel
{
    /// <summary>
    /// Tests for <see cref="ChangeLogEntryViewModel"/>
    /// </summary>
    public class ChangeLogEntryViewModelTest : TestBase
    {
        private readonly ChangeLogConfiguration m_DefaultConfiguration = ChangeLogConfigurationLoader.GetDefaultConfiguration();

        [Fact]
        public void Configuration_must_not_be_null()
        {
            Assert.Throws<ArgumentNullException>(() => new ChangeLogEntryViewModel(null!, GetChangeLogEntry()));
        }

        [Fact]
        public void Model_must_not_be_null()
        {
            Assert.Throws<ArgumentNullException>(() => new ChangeLogEntryViewModel(m_DefaultConfiguration, null!));
        }

        [Theory]
        [InlineData(null, "summary", "summary")]
        [InlineData("scope", "summary", "scope: summary")]
        public void Title_returns_expected_value(string scope, string summary, string expectedTitle)
        {
            // ARRANGE
            var model = GetChangeLogEntry(scope: scope, summary: summary);

            // ACT 
            var sut = new ChangeLogEntryViewModel(m_DefaultConfiguration, model);

            // ASSERT
            Assert.Equal(expectedTitle, sut.Title);
        }

        [Fact]
        public void Title_uses_configured_scope_display_name()
        {
            // ARRANGE
            var config = new ChangeLogConfiguration()
            {
                Scopes = new[]
                {
                    new ChangeLogConfiguration.ScopeConfiguration() { Name = "some-scope", DisplayName = "Scope Display Name"}
                }
            };
            var model = GetChangeLogEntry(scope: "some-scope", summary: "Some Summary");

            // ACT 
            var sut = new ChangeLogEntryViewModel(config, model);

            // ASSERT
            Assert.Equal("Scope Display Name: Some Summary", sut.Title);
        }

        [Fact]
        public void Footers_are_loaded_from_the_model()
        {
            // ARRANGE
            var footers = new[]
            {
                new ChangeLogEntryFooter(new CommitMessageFooterName("some-name"), "some value"),
                new ChangeLogEntryFooter(new CommitMessageFooterName("some-other-name"), "some other value") { WebUri = new Uri("http://example.com") }
            };

            var model = GetChangeLogEntry(footers: footers);

            // ACT 
            var sut = new ChangeLogEntryViewModel(m_DefaultConfiguration, model);

            // ASSERT
            Assert.NotEmpty(sut.Footers);
            Assert.Collection(sut.Footers,
                footer =>
                {
                    Assert.Equal(footers[0].Name.Value, footer.DisplayName);
                    Assert.Equal(footers[0].Value, footer.Value);
                    Assert.Equal(footers[0].WebUri, footer.WebUri);
                },
                footer =>
                {
                    Assert.Equal(footers[1].Name.Value, footer.DisplayName);
                    Assert.Equal(footers[1].Value, footer.Value);
                    Assert.Equal(footers[1].WebUri, footer.WebUri);
                },
                footer =>
                {
                    Assert.Equal("Commit", footer.DisplayName);
                    Assert.Equal(model.Commit.Id, footer.Value);
                    Assert.Equal(model.CommitWebUri, footer.WebUri);
                });
        }


        [Fact]
        public void Footers_does_not_include_breaking_changes()
        {
            // ARRANGE
            var footers = new[]
            {
                new ChangeLogEntryFooter(new CommitMessageFooterName("some-name"), "some value"),
                new ChangeLogEntryFooter(CommitMessageFooterName.BreakingChange, "some breaking change value")
            };

            var model = GetChangeLogEntry(footers: footers);

            // ACT 
            var sut = new ChangeLogEntryViewModel(m_DefaultConfiguration, model);

            // ASSERT
            Assert.NotEmpty(sut.Footers);
            Assert.Collection(sut.Footers,
                footer =>
                {
                    Assert.Equal(footers[0].Name.Value, footer.DisplayName);
                    Assert.Equal(footers[0].Value, footer.Value);
                    Assert.Equal(footers[0].WebUri, footer.WebUri);
                },
                footer =>
                {
                    Assert.Equal("Commit", footer.DisplayName);
                    Assert.Equal(model.Commit.Id, footer.Value);
                    Assert.Equal(model.CommitWebUri, footer.WebUri);
                });
        }


        [Theory]
        [InlineData(null)]
        [InlineData("http://example.com")]
        public void A_implicit_commit_footer_is_added(string? commitWebUri)
        {
            // ARRANGE
            var model = GetChangeLogEntry();
            model.CommitWebUri = String.IsNullOrEmpty(commitWebUri) ? null : new Uri(commitWebUri);

            // ACT 
            var sut = new ChangeLogEntryViewModel(m_DefaultConfiguration, model);

            // ASSERT
            Assert.NotEmpty(sut.Footers);
            Assert.Collection(sut.Footers,
                footer =>
                {
                    Assert.Equal("Commit", footer.DisplayName);
                    Assert.Equal(model.Commit.Id, footer.Value);
                    Assert.Equal(model.CommitWebUri, footer.WebUri);
                });
        }

        [Fact]
        public void BreakingChanges_is_empty_is_there_are_no_breaking_changes()
        {
            // ARRANGE
            var model = GetChangeLogEntry();

            // ACT
            var sut = new ChangeLogEntryViewModel(m_DefaultConfiguration, model);

            // ASSERT
            Assert.NotNull(sut.BreakingChanges);
            Assert.Empty(sut.BreakingChanges);
        }

        [Fact]
        public void BreakingChanges_returns_breaking_change_footers()
        {
            // ARRANGE
            var model = GetChangeLogEntry(breakingChangeDescriptions: new[]
            {
                "description 1",
                "description 3"
            });

            // ACT
            var sut = new ChangeLogEntryViewModel(m_DefaultConfiguration, model);

            // ASSERT
            Assert.NotNull(sut.BreakingChanges);
            Assert.Equal(2, sut.BreakingChanges.Count);
            Assert.Collection(sut.BreakingChanges,
                x =>
                {
                    Assert.Equal("description 1", x.Description);
                    Assert.Same(sut, x.Entry);
                    Assert.False(x.IsBreakingChangeFromHeader);
                },
                x =>
                {
                    Assert.Equal("description 3", x.Description);
                    Assert.Same(sut, x.Entry);
                    Assert.False(x.IsBreakingChangeFromHeader);
                });
        }

        [Fact]
        public void BreakingChanges_returns_a_single_item_if_the_entry_is_marked_as_breaking_change()
        {
            // ARRANGE
            var model = GetChangeLogEntry(summary: "summary 1", isBreakingChange: true);

            // ACT
            var sut = new ChangeLogEntryViewModel(m_DefaultConfiguration, model);

            // ASSERT
            Assert.NotNull(sut.BreakingChanges);
            var breakingChange = Assert.Single(sut.BreakingChanges);
            Assert.Equal("summary 1", breakingChange.Description);
            Assert.Same(sut, breakingChange.Entry);
            Assert.True(breakingChange.IsBreakingChangeFromHeader);
        }
    }
}
