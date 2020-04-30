using System;
using Grynwald.ChangeLog.Templates.ViewModel;
using Xunit;

namespace Grynwald.ChangeLog.Test.Templates.ViewModel
{
    /// <summary>
    /// Tests for <see cref="ChangeLogEntryViewModel"/>
    /// </summary>
    public class ChangeLogEntryViewModelTest : TestBase
    {
        [Fact]
        public void Model_must_not_be_null()
        {
            Assert.Throws<ArgumentNullException>(() => new ChangeLogEntryViewModel(null!));
        }

        [Theory]
        [InlineData(null, "summary", "summary")]
        [InlineData("scope", "summary", "scope: summary")]
        public void Title_returns_expected_value(string scope, string summary, string expectedTitle)
        {
            // ARRANGE
            var model = GetChangeLogEntry(scope: scope, summary: summary);

            // ACT 
            var sut = new ChangeLogEntryViewModel(model);

            // ASSERT
            Assert.Equal(expectedTitle, sut.Title);
        }
    }
}
