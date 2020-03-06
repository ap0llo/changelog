using System.Linq;
using ChangeLogCreator.Model;
using ChangeLogCreator.Tasks;
using Moq;
using Xunit;

namespace ChangeLogCreator.Test
{
    public class ChangeLogPipelineTest
    {
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        public void Run_executes_all_tasks_in_the_insertion_order(int numberOfTasks)
        {
            // ARRANGE
            var tasks = Enumerable.Range(0, numberOfTasks)
                .Select(_ =>
                {
                    var mock = new Mock<IChangeLogTask>(MockBehavior.Strict);
                    mock.Setup(x => x.Run(It.IsAny<ChangeLog>()));
                    return mock;
                })
                .ToArray();

            var sut = new ChangeLogPipeline();

            foreach (var taskMock in tasks)
            {
                sut.AddTask(taskMock.Object);
            }

            // ACT 
            var changeLog = sut.Run();

            // ASSERT
            Assert.NotNull(changeLog);
            Assert.All(tasks, task => task.Verify(x => x.Run(It.IsAny<ChangeLog>()), Times.Once));
        }
    }
}
