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
        public class Success
        {
            [Fact]
            public void Is_false_for_error_result()
            {
                // ARRANGE
                var executedTasks = Array.Empty<ChangeLogTaskExecutionResult>();
                var pendingTasks = Array.Empty<IChangeLogTask>();
                var errorResult = ChangeLogPipelineResult.CreateErrorResult(executedTasks, pendingTasks);

                // ACT
                var success = errorResult.Success;

                // ASSERT
                Assert.False(success);
            }


            [Fact]
            public void Is_true_for_success_result()
            {
                // ARRANGE
                var changeLog = new ApplicationChangeLog();
                var executedTasks = Array.Empty<ChangeLogTaskExecutionResult>();
                var pendingTasks = Array.Empty<IChangeLogTask>();
                var successResult = ChangeLogPipelineResult.CreateSuccessResult(executedTasks, pendingTasks, changeLog);

                // ACT
                var success = successResult.Success;

                // ASSERT
                Assert.True(success);
            }

        }

        public class Value
        {
            [Fact]
            public void Throws_InvalidOperationException_for_error_result()
            {
                // ARRANGE
                var executedTasks = Array.Empty<ChangeLogTaskExecutionResult>();
                var pendingTasks = Array.Empty<IChangeLogTask>();
                var errorResult = ChangeLogPipelineResult.CreateErrorResult(executedTasks, pendingTasks);

                // ACT
                var ex = Record.Exception(() => errorResult.Value);

                // ASSERT
                Assert.IsType<InvalidOperationException>(ex);
            }

            [Fact]
            public void Returns_value_for_success_result()
            {
                // ARRANGE
                var changeLog = new ApplicationChangeLog();
                var executedTasks = Array.Empty<ChangeLogTaskExecutionResult>();
                var pendingTasks = Array.Empty<IChangeLogTask>();
                var successResult = ChangeLogPipelineResult.CreateSuccessResult(executedTasks, pendingTasks, changeLog);

                // ACT
                var value = successResult.Value;

                // ASSERT
                Assert.Same(changeLog, value);
            }
        }

        public class CreateErrorResult
        {
            [Fact]
            public void Checks_executed_tasks_parameter_for_null()
            {
                // ARRANGE
                var pendingTasks = Array.Empty<IChangeLogTask>();

                // ACT 
                var ex = Record.Exception(() => ChangeLogPipelineResult.CreateErrorResult(null!, pendingTasks));

                // ASSERT
                var argumentNullException = Assert.IsType<ArgumentNullException>(ex);
                Assert.Equal("executedTasks", argumentNullException.ParamName);
            }

            [Fact]
            public void Checks_pending_tasks_parameter_for_null()
            {
                // ARRANGE
                var executedTasks = Array.Empty<ChangeLogTaskExecutionResult>();

                // ACT 
                var ex = Record.Exception(() => ChangeLogPipelineResult.CreateErrorResult(executedTasks, null!));

                // ASSERT
                var argumentNullException = Assert.IsType<ArgumentNullException>(ex);
                Assert.Equal("pendingTasks", argumentNullException.ParamName);
            }
        }

        public class CreateSuccessResult
        {
            [Fact]
            public void Checks_executed_tasks_parameter_for_null()
            {
                // ARRANGE
                var value = new ApplicationChangeLog();
                var pendingTasks = Array.Empty<IChangeLogTask>();

                // ACT 
                var ex = Record.Exception(() => ChangeLogPipelineResult.CreateSuccessResult(null!, pendingTasks, value));

                // ASSERT
                var argumentNullException = Assert.IsType<ArgumentNullException>(ex);
                Assert.Equal("executedTasks", argumentNullException.ParamName);
            }

            [Fact]
            public void Checks_pending_tasks_parameter_for_null()
            {
                // ARRANGE
                var value = new ApplicationChangeLog();
                var executedTasks = Array.Empty<ChangeLogTaskExecutionResult>();

                // ACT 
                var ex = Record.Exception(() => ChangeLogPipelineResult.CreateSuccessResult(executedTasks, null!, value));

                // ASSERT
                var argumentNullException = Assert.IsType<ArgumentNullException>(ex);
                Assert.Equal("pendingTasks", argumentNullException.ParamName);
            }

            [Fact]
            public void Checks_value_parameter_for_null()
            {
                // ARRANGE
                var executedTasks = Array.Empty<ChangeLogTaskExecutionResult>();
                var pendingTasks = Array.Empty<IChangeLogTask>();

                // ACT 
                var ex = Record.Exception(() => ChangeLogPipelineResult.CreateSuccessResult(executedTasks, pendingTasks, null!));

                // ASSERT
                var argumentNullException = Assert.IsType<ArgumentNullException>(ex);
                Assert.Equal("value", argumentNullException.ParamName);
            }
        }

    }
}
