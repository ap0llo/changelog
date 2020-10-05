using System.Collections.Generic;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.ConventionalCommits;
using Grynwald.ChangeLog.Model;
using Grynwald.ChangeLog.Templates;
using Xunit;

namespace Grynwald.ChangeLog.Test.Templates
{
    /// <summary>
    /// Unit tests for <see cref="ModelExtensions"/>
    /// </summary>
    public class ModelExtensionsTest : TestBase
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void GetScopeDisplayName_returns_scope_if_scope_is_null_or_empty(string scope)
        {
            // ARRANGE
            var changeLogEntry = GetChangeLogEntry(scope: scope);

            // ACT 
            var displayName = changeLogEntry.GetScopeDisplayName(new ChangeLogConfiguration());

            // ASSERT
            Assert.Null(displayName);
        }

        [Theory]
        [InlineData("someScope", "someScope")]
        [InlineData("SOMESCOPE", "someScope")] // Scope must be compared case-insensitive
        [InlineData("someScope", "SOMESCOPE")] // Scope must be compared case-insensitive
        public void GetScopeDisplayName_returns_expected_display_name(string configuredScope, string scope)
        {
            // ARRANGE
            var config = new ChangeLogConfiguration()
            {
                Scopes = new Dictionary<string, ChangeLogConfiguration.ScopeConfiguration>()
                {
                    { configuredScope, new ChangeLogConfiguration.ScopeConfiguration() { DisplayName = "Scope Display Name" } }
                }
            };
            var changeLogEntry = GetChangeLogEntry(scope: scope);

            // ACT 
            var displayName = changeLogEntry.GetScopeDisplayName(config);

            // ASSERT
            Assert.Equal("Scope Display Name", displayName);
        }

        [Theory]
        [InlineData("")]
        [InlineData("\t")]
        [InlineData("  ")]
        [InlineData(null)]
        public void GetScopeDisplayName_returns_scope_if_display_name_is_empty(string displayName)
        {
            // ARRANGE
            var config = new ChangeLogConfiguration()
            {
                Scopes = new Dictionary<string, ChangeLogConfiguration.ScopeConfiguration>()
                {
                    { "someScope",  new ChangeLogConfiguration.ScopeConfiguration() { DisplayName = displayName } }
                }
            };
            var changeLogEntry = GetChangeLogEntry(scope: "someScope");

            // ACT 
            var actualDisplayName = changeLogEntry.GetScopeDisplayName(config);

            // ASSERT
            Assert.Equal("someScope", actualDisplayName);
        }

        [Fact]
        public void GetScopeDisplayName_returns_scope_is_no_matching_configuration_is_found()
        {
            // ARRANGE
            var config = new ChangeLogConfiguration()
            {
                Scopes = new Dictionary<string, ChangeLogConfiguration.ScopeConfiguration>()
                {
                    { "someScope",  new ChangeLogConfiguration.ScopeConfiguration() { DisplayName = "Scope Display Name" } }
                }
            };
            var changeLogEntry = GetChangeLogEntry(scope: "someScopeOther");

            // ACT 
            var displayName = changeLogEntry.GetScopeDisplayName(config);

            // ASSERT
            Assert.Equal("someScopeOther", displayName);
        }


        [Theory]
        [InlineData("footerName", "footerName")]
        [InlineData("FOOTERNAME", "footerName")]    // Scope must be compared case-insensitive        
        [InlineData("footerName", "FOOTERNAME")]    // Scope must be compared case-insensitive            
        public void GetFooterDisplayName_returns_expected_display_name(string configuredFooter, string footerName)
        {
            // ARRANGE
            var configuration = new ChangeLogConfiguration()
            {
                Footers = new Dictionary<string, ChangeLogConfiguration.FooterConfiguration>()
                {
                    { configuredFooter, new ChangeLogConfiguration.FooterConfiguration() { DisplayName = "Footer Display Name" } }
                }
            };

            var footer = new ChangeLogEntryFooter(new CommitMessageFooterName(footerName), "Irrelevant");

            // ACT
            var displayName = footer.GetFooterDisplayName(configuration);

            // ASSERT
            Assert.Equal("Footer Display Name", displayName);
        }


        [Fact]
        public void GetFooterDisplayName_returns_footer_name_if_no_display_name_is_configured()
        {
            // ARRANGE
            var footerName = "footerName";
            var configuration = new ChangeLogConfiguration()
            {
                Footers = new Dictionary<string, ChangeLogConfiguration.FooterConfiguration>()
                {
                    { "someOtherFooter",  new ChangeLogConfiguration.FooterConfiguration() { DisplayName = "Footer Display Name" } }
                }
            };

            var footer = new ChangeLogEntryFooter(new CommitMessageFooterName(footerName), "Irrelevant");

            // ACT
            var displayName = footer.GetFooterDisplayName(configuration);

            // ASSERT
            Assert.Equal(footerName, displayName);
        }

        [Theory]
        [InlineData("")]
        [InlineData("\t")]
        [InlineData("  ")]
        [InlineData(null)]
        public void GetFooterDisplayName_returns_footer_name_if_display_name_is_empty(string configuredDisplayName)
        {
            // ARRANGE
            var footerName = "footerName";
            var configuration = new ChangeLogConfiguration()
            {
                Footers = new Dictionary<string, ChangeLogConfiguration.FooterConfiguration>()
                {
                    { footerName, new ChangeLogConfiguration.FooterConfiguration() { DisplayName = configuredDisplayName } }
                }
            };

            var footer = new ChangeLogEntryFooter(new CommitMessageFooterName(footerName), "Irrelevant");

            // ACT
            var displayName = footer.GetFooterDisplayName(configuration);

            // ASSERT
            Assert.Equal(footerName, displayName);
        }

    }
}
