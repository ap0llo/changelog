using System;
using System.Linq;
using System.Threading.Tasks;
using Grynwald.ChangeLog.Model;
using Grynwald.ChangeLog.Pipeline;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace Grynwald.ChangeLog.Test.Pipeline
{
    /// <summary>
    /// Tests for <see cref="ChangeLogPipeline"/>
    /// </summary>
    public class ChangeLogPipelineTest
    {
        private readonly ILogger<ChangeLogPipeline> m_Logger;


        public ChangeLogPipelineTest(ITestOutputHelper testOutputHelper)
        {
            m_Logger = new XunitLogger<ChangeLogPipeline>(testOutputHelper);
        }


        [Fact]
        public void Logger_must_not_be_null()
        {
            // ARRANGE

            // ACT 
            var ex = Record.Exception(() => new ChangeLogPipeline(logger: null!, tasks: new[] { Mock.Of<IChangeLogTask>() }));

            // ASSERT
            var argumentNullException = Assert.IsType<ArgumentNullException>(ex);
            Assert.Equal("logger", argumentNullException.ParamName);
        }

        [Fact]
        public void Tasks_must_not_be_null()
        {
            // ARRANGE

            // ACT 
            var ex = Record.Exception(() => new ChangeLogPipeline(logger: m_Logger, tasks: null!));

            // ASSERT
            var argumentNullException = Assert.IsType<ArgumentNullException>(ex);
            Assert.Equal("tasks", argumentNullException.ParamName);
        }

        [Fact]
        public void Tasks_must_not_be_empty()
        {
            // ARRANGE

            // ACT 
            var ex = Record.Exception(() => new ChangeLogPipeline(logger: m_Logger, tasks: Array.Empty<IChangeLogTask>()));

            // ASSERT
            var argumentException = Assert.IsType<ArgumentException>(ex);
            Assert.Equal("tasks", argumentException.ParamName);
            Assert.Contains("Task list must not be empty", ex.Message);
        }


        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public async Task Run_executes_all_tasks_in_the_insertion_order(int numberOfTasks)
        {
            // ARRANGE
            var tasks = Enumerable.Range(0, numberOfTasks)
                .Select(_ =>
                {
                    var mock = new Mock<IChangeLogTask>(MockBehavior.Strict);
                    mock.Setup(x => x.RunAsync(It.IsAny<ApplicationChangeLog>())).Returns(Task.FromResult(ChangeLogTaskResult.Success));
                    return mock;
                })
                .ToArray();

            var sut = new ChangeLogPipeline(m_Logger, tasks.Select(x => x.Object).ToArray());

            // ACT 
            var result = await sut.RunAsync();

            // ASSERT
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Value);
            Assert.All(tasks, task => task.Verify(x => x.RunAsync(It.IsAny<ApplicationChangeLog>()), Times.Once));
            Assert.Collection(
                result.ExecutedTasks,
                tasks.Select<Mock<IChangeLogTask>, Action<ChangeLogTaskExecutionResult>>(taskMock => r =>
                {
                    Assert.Same(taskMock.Object, r.Task);
                    Assert.Equal(ChangeLogTaskResult.Success, r.Result);
                }).ToArray()
            );
            Assert.Empty(result.PendingTasks);
        }

        [Fact]
        public async Task Run_aborts_execution_if_a_task_fails()
        {
            // ARRANGE
            var tasks = Enumerable.Range(0, 3)
                .Select(_ =>
                {
                    var mock = new Mock<IChangeLogTask>(MockBehavior.Strict);
                    mock.Setup(x => x.RunAsync(It.IsAny<ApplicationChangeLog>())).Returns(Task.FromResult(ChangeLogTaskResult.Success));
                    return mock;
                })
                .ToArray();

            tasks[1].Setup(x => x.RunAsync(It.IsAny<ApplicationChangeLog>())).Returns(Task.FromResult(ChangeLogTaskResult.Error));

            var sut = new ChangeLogPipeline(m_Logger, tasks.Select(x => x.Object).ToArray());

            // ACT 
            var result = await sut.RunAsync();

            // ASSERT
            Assert.NotNull(result);
            Assert.False(result.Success);
            tasks[0].Verify(x => x.RunAsync(It.IsAny<ApplicationChangeLog>()), Times.Once);
            tasks[1].Verify(x => x.RunAsync(It.IsAny<ApplicationChangeLog>()), Times.Once);
            tasks[2].Verify(x => x.RunAsync(It.IsAny<ApplicationChangeLog>()), Times.Never);

            Assert.Collection(
                result.ExecutedTasks,
                executedTask =>
                {
                    Assert.Same(tasks[0].Object, executedTask.Task);
                    Assert.Equal(ChangeLogTaskResult.Success, executedTask.Result);
                },
                executedTask =>
                {
                    Assert.Same(tasks[1].Object, executedTask.Task);
                    Assert.Equal(ChangeLogTaskResult.Error, executedTask.Result);
                }
            );

            Assert.Collection(
                result.PendingTasks,
                pendingTask => Assert.Same(tasks[2].Object, pendingTask)
            );
        }

        [Fact]
        public async Task Run_continues_execution_if_a_task_is_skipped()
        {
            // ARRANGE
            var tasks = Enumerable.Range(0, 3)
                .Select(_ =>
                {
                    var mock = new Mock<IChangeLogTask>(MockBehavior.Strict);
                    mock.Setup(x => x.RunAsync(It.IsAny<ApplicationChangeLog>())).Returns(Task.FromResult(ChangeLogTaskResult.Success));
                    return mock;
                })
                .ToArray();

            tasks[1].Setup(x => x.RunAsync(It.IsAny<ApplicationChangeLog>())).Returns(Task.FromResult(ChangeLogTaskResult.Skipped));

            var sut = new ChangeLogPipeline(m_Logger, tasks.Select(x => x.Object).ToArray());

            // ACT 
            var result = await sut.RunAsync();

            // ASSERT
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Value);
            Assert.All(tasks, task => task.Verify(x => x.RunAsync(It.IsAny<ApplicationChangeLog>()), Times.Once));

            Assert.Collection(
                result.ExecutedTasks,
                executedTask =>
                {
                    Assert.Same(tasks[0].Object, executedTask.Task);
                    Assert.Equal(ChangeLogTaskResult.Success, executedTask.Result);
                },
                executedTask =>
                {
                    Assert.Same(tasks[1].Object, executedTask.Task);
                    Assert.Equal(ChangeLogTaskResult.Skipped, executedTask.Result);
                },
                executedTask =>
                {
                    Assert.Same(tasks[2].Object, executedTask.Task);
                    Assert.Equal(ChangeLogTaskResult.Success, executedTask.Result);
                }
            );

            Assert.Empty(result.PendingTasks);
        }

        private abstract class TestTaskBase : IChangeLogTask
        {
            public virtual Task<ChangeLogTaskResult> RunAsync(ApplicationChangeLog changeLog) => Task.FromResult(ChangeLogTaskResult.Success);
        }


        private class TestTask1 : TestTaskBase
        {
        }

        [BeforeTask(typeof(TestTask1))]
        private class TestTask2 : TestTaskBase
        {
        }

        [AfterTask(typeof(TestTask2))]
        private class TestTask3 : TestTaskBase
        {
        }

        private class TestTask4 : TestTaskBase
        {
        }

        [Fact]
        public async Task Run_executes_a_tasks_dependencies_before_running_the_task()
        {
            // ARRANGE
            var task1 = new TestTask1();
            var task2 = new TestTask2();
            var task3 = new TestTask3();
            var task4 = new TestTask4();
            var sut = new ChangeLogPipeline(m_Logger, new IChangeLogTask[] { task1, task2, task3, task4 });

            // ACT
            var result = await sut.RunAsync();

            // ASSERT
            Assert.True(result.Success);

            Assert.Collection(
                result.ExecutedTasks,
                executedTask => Assert.Same(task2, executedTask.Task),
                executedTask => Assert.Same(task1, executedTask.Task),
                executedTask => Assert.Same(task3, executedTask.Task),
                executedTask => Assert.Same(task4, executedTask.Task)
            );

            Assert.Empty(result.PendingTasks);
        }


        [BeforeTask(typeof(TestTask5))]
        private class TestTask5 : TestTaskBase
        { }


        [Fact]
        public async Task Run_throws_InvalidPipelineConfigurationException_if_there_are_cyclic_dependencies_between_tasks_01()
        {
            // ARRANGE
            var tasks = new IChangeLogTask[]
            {
                new TestTask5()
            };
            var sut = new ChangeLogPipeline(m_Logger, tasks);

            // ACT 
            var ex = await Record.ExceptionAsync(async () => await sut.RunAsync());

            // ASSERT
            Assert.IsType<InvalidPipelineConfigurationException>(ex);
            Assert.Equal("Detected circular dependency between tasks: 'TestTask5' -> 'TestTask5'", ex.Message);

        }

        [AfterTask(typeof(TestTask7))]
        [BeforeTask(typeof(TestTask9))]
        private class TestTask6 : TestTaskBase
        { }

        [AfterTask(typeof(TestTask8))]
        private class TestTask7 : TestTaskBase
        { }

        [AfterTask(typeof(TestTask9))]
        private class TestTask8 : TestTaskBase
        { }

        private class TestTask9 : TestTaskBase
        { }

        [Fact]
        public async Task Run_throws_InvalidPipelineConfigurationException_if_there_are_cyclic_dependencies_between_tasks_02()
        {
            // ARRANGE
            var tasks = new IChangeLogTask[]
            {
                new TestTask6(),
                new TestTask7(),
                new TestTask8(),
                new TestTask9(),
            };
            var sut = new ChangeLogPipeline(m_Logger, tasks);

            // ACT 
            var ex = await Record.ExceptionAsync(async () => await sut.RunAsync());

            // ASSERT
            Assert.IsType<InvalidPipelineConfigurationException>(ex);
            Assert.Equal("Detected circular dependency between tasks: 'TestTask6' -> 'TestTask7' -> 'TestTask8' -> 'TestTask9' -> 'TestTask6'", ex.Message);
        }

        private class TestTask10 : TestTaskBase
        { }

        [AfterTask(typeof(TestTask10))]
        private class TestTask11 : TestTaskBase
        { }

        [Fact]
        public async Task Run_throws_InvalidPipelineConfigurationException_if_there_is_a_dependency_to_a_task_not_added_to_the_pipeline_01()
        {
            // ARRANGE
            var tasks = new IChangeLogTask[]
            {
                new TestTask11(),
            };
            var sut = new ChangeLogPipeline(m_Logger, tasks);

            // ACT 
            var ex = await Record.ExceptionAsync(async () => await sut.RunAsync());

            // ASSERT
            Assert.IsType<InvalidPipelineConfigurationException>(ex);
            Assert.Equal("Dependency 'TestTask10' of task 'TestTask11' was not found", ex.Message);

        }

        [BeforeTask(typeof(TestTask13))]
        private class TestTask12 : TestTaskBase
        { }

        private class TestTask13 : TestTaskBase
        { }


        [Fact]
        public async Task Run_throws_InvalidPipelineConfigurationException_if_there_is_a_dependency_to_task_not_added_to_the_pipeline_02()
        {
            // ARRANGE
            var tasks = new IChangeLogTask[]
            {
                new TestTask12(),
            };
            var sut = new ChangeLogPipeline(m_Logger, tasks);

            // ACT 
            var ex = await Record.ExceptionAsync(async () => await sut.RunAsync());

            // ASSERT
            Assert.IsType<InvalidPipelineConfigurationException>(ex);
            Assert.Equal("Dependent task 'TestTask13' of task 'TestTask12' was not found", ex.Message);

        }



        private class TestTask14 : TestTaskBase
        {
            private readonly string m_Name;

            public TestTask14(string name)
            {
                m_Name = name;
            }

            public override string ToString() => m_Name;
        }

        [AfterTask(typeof(TestTask14))]
        private class TestTask15 : TestTaskBase
        {
            private readonly string m_Name;

            public TestTask15(string name)
            {
                m_Name = name;
            }

            public override string ToString() => m_Name;
        }


        [Fact]
        public async Task Run_executes_tasks_in_the_expected_order_if_there_are_multiple_tasks_of_the_same_type()
        {
            // ARRANGE
            var task1 = new TestTask15("task1");
            var task2 = new TestTask14("task2");
            var task3 = new TestTask15("task3");
            var task4 = new TestTask14("task4");

            var sut = new ChangeLogPipeline(m_Logger, new IChangeLogTask[] { task1, task2, task3, task4 });

            // ACT
            var result = await sut.RunAsync();

            // ASSERT
            Assert.True(result.Success);

            Assert.Collection(
                result.ExecutedTasks,
                executedTask => Assert.Same(task2, executedTask.Task),
                executedTask => Assert.Same(task4, executedTask.Task),
                executedTask => Assert.Same(task1, executedTask.Task),
                executedTask => Assert.Same(task3, executedTask.Task)
            );

            Assert.Empty(result.PendingTasks);
        }
    }
}
