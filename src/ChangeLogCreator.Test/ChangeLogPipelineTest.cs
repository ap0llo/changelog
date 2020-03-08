using System.Linq;
using System.Threading.Tasks;
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
        public async Task Run_executes_all_tasks_in_the_insertion_order(int numberOfTasks)
        {
            // ARRANGE
            var tasks = Enumerable.Range(0, numberOfTasks)
                .Select(_ =>
                {
                    var mock = new Mock<IChangeLogTask>(MockBehavior.Strict);
                    mock.Setup(x => x.RunAsync(It.IsAny<ChangeLog>())).Returns(Task.CompletedTask);
                    return mock;
                })
                .ToArray();

            var sut = new ChangeLogPipeline();

            foreach (var taskMock in tasks)
            {
                sut.AddTask(taskMock.Object);
            }

            // ACT 
            var changeLog = await sut.RunAsync();

            // ASSERT
            Assert.NotNull(changeLog);
            Assert.All(tasks, task => task.Verify(x => x.RunAsync(It.IsAny<ChangeLog>()), Times.Once));
        }
    }
}
