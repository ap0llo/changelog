using System;
using Grynwald.ChangeLog.Model.Text;
using Xunit;

namespace Grynwald.ChangeLog.Test.Model.Text
{
    /// <summary>
    /// Tests for <see cref="ChangeLogEntryReferenceTextElement"/>
    /// </summary>
    public class ChangeLogEntryReferenceTextElementTest : TestBase
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        public void Text_must_not_be_null_or_whitespace(string? text)
        {
            // ARRANGE
            var entry = GetChangeLogEntry();

            // ACT 
            var ex = Record.Exception(() => new ChangeLogEntryReferenceTextElement(text!, entry));

            // ASSERT
            var argumentException = Assert.IsType<ArgumentException>(ex);
            Assert.Equal("text", argumentException.ParamName);
        }

        [Fact]
        public void Entry_must_not_be_null()
        {
            // ARRANGE

            // ACT 
            var ex = Record.Exception(() => new ChangeLogEntryReferenceTextElement("some-text", null!));

            // ASSERT
            var argumentException = Assert.IsType<ArgumentNullException>(ex);
            Assert.Equal("entry", argumentException.ParamName);
        }
    }
}
