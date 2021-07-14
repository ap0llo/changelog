using System;
using System.Threading.Tasks;
using Autofac;
using Grynwald.ChangeLog.Model;
using Grynwald.ChangeLog.Pipeline;
using Grynwald.ChangeLog.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Grynwald.ChangeLog.Test.Pipeline
{
    /// <summary>
    /// Tests for <see cref="ChangeLogPipelineBuilder"/>
    /// </summary>
    public class ChangeLogPipelineBuilderTest : ContainerTestBase
    {
        [Fact]
        public void Container_must_not_be_null()
        {
            Assert.Throws<ArgumentNullException>(() => new ChangeLogPipelineBuilder(null!));
        }

        [Fact]
        public void Constructor_initializes_Container_property()
        {
            // ARRANGE
            var container = Mock.Of<IContainer>(MockBehavior.Strict);

            // ACT 
            var pipelineBuilder = new ChangeLogPipelineBuilder(container);

            // ASSERT
            Assert.NotNull(pipelineBuilder.Container);
            Assert.Same(container, pipelineBuilder.Container);
        }

        private class TestTask : IChangeLogTask
        {
            public Task<ChangeLogTaskResult> RunAsync(ApplicationChangeLog changeLog) => throw new NotImplementedException();
        }

        [Fact]
        public void AddTask_resolves_task_from_the_container()
        {
            // ARRANGE
            var task = new TestTask();

            using var container = BuildContainer(b =>
            {
                b.RegisterInstance(task);
            });

            var pipelineBuilder = new ChangeLogPipelineBuilder(container);

            // ACT
            _ = pipelineBuilder.AddTask<TestTask>();

            // ASSERT
            var registeredTask = Assert.Single(pipelineBuilder.Tasks);
            Assert.Same(task, registeredTask);
        }

        [Fact]
        public void Build_creates_a_ChangeLogPipeline_using_the_container()
        {
            // ARRANGE
            var task = new TestTask();

            using var container = BuildContainer(b =>
            {
                b.RegisterType<ChangeLogPipeline>();
                b.RegisterInstance(NullLogger<ChangeLogPipeline>.Instance).As<ILogger<ChangeLogPipeline>>();
                b.RegisterInstance(task);
            });

            var pipelineBuilder = new ChangeLogPipelineBuilder(container);
            _ = pipelineBuilder.AddTask<TestTask>();

            // ACT
            var pipeline = pipelineBuilder.Build();

            // ASSERT
            Assert.NotNull(pipeline);
            var registeredTask = Assert.Single(pipeline.Tasks);
            Assert.Same(task, registeredTask);
        }
    }
}
