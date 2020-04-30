using System;
using Grynwald.ChangeLog.Templates.ViewModel;
using Xunit;

namespace Grynwald.ChangeLog.Test.Templates.ViewModel
{
    /// <summary>
    /// Tests for <see cref="BreakingChangeViewModel"/>
    /// </summary>
    public class BreakingChangeViewModelTest : TestBase
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        public void Description_must_not_be_null_or_whitespace(string description)
        {
            Assert.Throws<ArgumentException>(() => new BreakingChangeViewModel(description, new ChangeLogEntryViewModel(GetChangeLogEntry())));
        }

        [Fact]
        public void Entry_must_not_be_null()
        {
            Assert.Throws<ArgumentNullException>(() => new BreakingChangeViewModel("Some description", null!));
        }
    }
}
