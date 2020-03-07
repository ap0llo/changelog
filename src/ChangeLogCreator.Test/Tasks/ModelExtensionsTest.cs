using ChangeLogCreator.Configuration;
using ChangeLogCreator.Tasks;
using Xunit;

namespace ChangeLogCreator.Test.Tasks
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
                Scopes = new[]
                {
                    new ChangeLogConfiguration.ScopeConfiguration() { Name = configuredScope, DisplayName = "Scope Display Name" }
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
                Scopes = new[]
                {
                    new ChangeLogConfiguration.ScopeConfiguration() { Name = "someScope", DisplayName = displayName }
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
                Scopes = new[]
                {
                    new ChangeLogConfiguration.ScopeConfiguration() { Name = "someScope", DisplayName = "Scope Display Name" }
                }
            };
            var changeLogEntry = GetChangeLogEntry(scope: "someScopeOther");

            // ACT 
            var displayName = changeLogEntry.GetScopeDisplayName(config);

            // ASSERT
            Assert.Equal("someScopeOther", displayName);
        }
    }
}
