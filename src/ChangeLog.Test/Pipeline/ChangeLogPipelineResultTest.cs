using System;
using Grynwald.ChangeLog.Model;
using Grynwald.ChangeLog.Pipeline;
using Xunit;

namespace Grynwald.ChangeLog.Test.Pipeline
{
    /// <summary>
    /// Tests for <see cref="ChangeLogPipelineResult"/>
    /// </summary>
    public class ChangeLogPipelineResultTest
    {
        [Fact]
        public void Error_result_throws_InvalidOperationException_when_accessing_the_Value()
        {
            // ARRANGE

            // ACT
            var errorResult = ChangeLogPipelineResult.CreateErrorResult();

            // ASSERT
            Assert.False(errorResult.Success);
            Assert.Throws<InvalidOperationException>(() => errorResult.Value);
        }

        [Fact]
        public void Success_result_returns_expected_value()
        {
            // ARRANGE
            var changeLog = new ApplicationChangeLog();
            var successResult = ChangeLogPipelineResult.CreateSuccessResult(changeLog);

            // ACT / ASSERT
            Assert.True(successResult.Success);
            Assert.Same(changeLog, successResult.Value);
        }

        [Fact]
        public void CreateSuccessResult_checks_value_for_null()
        {
            Assert.Throws<ArgumentNullException>(() => ChangeLogPipelineResult.CreateSuccessResult(null!));
        }
    }
}
