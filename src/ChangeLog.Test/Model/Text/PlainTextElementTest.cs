using System;
using Grynwald.ChangeLog.Model.Text;
using Xunit;

namespace Grynwald.ChangeLog.Test.Model.Text
{
    /// <summary>
    /// Tests for <see cref="PlainTextElement"/>
    /// </summary>
    public class PlainTextElementTest
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        public void Text_must_not_be_null_or_whitespace(string text)
        {
            // ARRANGE

            // ACT 
            var ex = Record.Exception(() => new PlainTextElement(text));

            // ASSERT
            var argumentException = Assert.IsType<ArgumentException>(ex);
            Assert.Equal("text", argumentException.ParamName);
        }
    }
}
