using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Grynwald.ChangeLog.Model;
using Microsoft.Extensions.Logging;

namespace Grynwald.ChangeLog.Pipeline
{
    public sealed class ChangeLogPipeline
    {
        private readonly IReadOnlyList<IChangeLogTask> m_Tasks;
        private readonly ILogger<ChangeLogPipeline> m_Logger;


        public IEnumerable<IChangeLogTask> Tasks => m_Tasks;


        public ChangeLogPipeline(ILogger<ChangeLogPipeline> logger, IEnumerable<IChangeLogTask> tasks)
        {
            m_Tasks = (tasks ?? throw new ArgumentNullException(nameof(tasks))).ToList();
            m_Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        public async Task<ChangeLogPipelineResult> RunAsync()
        {
            var changeLog = new ApplicationChangeLog();
            var executedTasks = new List<ChangeLogTaskExecutionResult>();

            var pendingTasks = GetTasksInExecutionOrder();

            while (pendingTasks.Count > 0)
            {
                var task = pendingTasks.Dequeue();
                var taskName = GetTaskName(task);

                m_Logger.LogDebug($"Running task '{taskName}'");
                var result = await task.RunAsync(changeLog);
                m_Logger.LogDebug($"Task '{taskName}' completed with result '{result}'");

                executedTasks.Add(new ChangeLogTaskExecutionResult(task, result));

                if (result == ChangeLogTaskResult.Error)
                {
                    m_Logger.LogDebug($"Task '{taskName}' failed, aborting execution of pipeline.");
                    return ChangeLogPipelineResult.CreateErrorResult(executedTasks, pendingTasks.ToList());
                }
            }
            return ChangeLogPipelineResult.CreateSuccessResult(executedTasks, Array.Empty<IChangeLogTask>(), changeLog);
        }


        private Graph<IChangeLogTask> GetTaskDependencyGraph()
        {
            var graph = new Graph<IChangeLogTask>();

            // Add each graph as node
            foreach (var task in m_Tasks)
            {
                graph.AddNode(task);
            }

            // Add all dependencies between graphs as edges to the graph
            foreach (var task in m_Tasks)
            {
                // The [AfterTask] attribute defines that the task the attribute is added to should run after the referenced task, e.g.
                //
                // [AfterTask(typeof(Task1))]
                // public class Task2 { }
                //
                // Task1 must run before Task2, thus resulting in an Task1 -> Task2 edge in the graph
                //
                var dependencyTypes = task.GetType().GetCustomAttributes<AfterTaskAttribute>().Select(x => x.TaskType);
                var dependencies = new List<IChangeLogTask>();
                foreach (var type in dependencyTypes)
                {
                    // Evaluation of dependencies only matches the type declared using the [AfterTask] attribute exactly
                    // and does not consider derived types.
                    // There is a test in ArchTest.cs that ensure that all tasks referenced using a [AfterTask] attribute
                    // are sealed. Thus, the simple '==' comparison of the types is sufficient
                    var matchingTasks = m_Tasks.Where(t => t.GetType() == type);
                    if (!matchingTasks.Any())
                    {
                        throw new InvalidPipelineConfigurationException($"Dependency '{type.Name}' of task '{GetTaskName(task)}' was not found");
                    }

                    dependencies.AddRange(matchingTasks);
                }

                // Add edge to all tasks of the type specified in the [AfterTask] attribute
                foreach (var dependency in dependencies)
                {
                    graph.AddEdge(task, dependency);
                }

                // The [BeforeTask] attribute defines that the task the attribute is added to should run before the referenced task, e.g.
                //
                // [BeforeTask(typeof(Task1))]
                // public class Task2 { }
                //
                // Task2 must run before Task1, thus resulting in an Task1 <- Task2 edge in the graph
                //
                var dependentTypes = task.GetType().GetCustomAttributes<BeforeTaskAttribute>().Select(x => x.TaskType).ToHashSet();
                var dependents = new List<IChangeLogTask>();
                foreach (var type in dependentTypes)
                {
                    // Evaluation of dependencies only matches the type declared using the [BeforeTask] attribute exactly
                    // and does not consider derived types.
                    // There is a test in ArchTest.cs that ensure that all tasks referenced using a [BeforeTask] attribute
                    // are sealed. Thus, the simple '==' comparison of the types is sufficient
                    var matchingTasks = m_Tasks.Where(t => t.GetType() == type);
                    if (!matchingTasks.Any())
                    {
                        throw new InvalidPipelineConfigurationException($"Dependent task '{type.Name}' of task '{GetTaskName(task)}' was not found");
                    }

                    dependents.AddRange(matchingTasks);
                }
                // Add edge from all tasks of the type specified in the [BeforeTask] attribute
                foreach (var dependent in dependents)
                {
                    graph.AddEdge(dependent, task);
                }
            }


            //TODO 2021-07-13: Write graph to log (as DEBUG message)
            return graph;
        }

        private Queue<IChangeLogTask> GetTasksInExecutionOrder()
        {
            var dependencyGraph = GetTaskDependencyGraph();

            // Save an index for each of the task
            // When two tasks could be executed interchangeably with regards to the dependencies
            // they will be executed in the insertion order
            var taskByInsertionIndex = new Dictionary<IChangeLogTask, int>();
            var i = 0;
            foreach (var task in m_Tasks)
            {
                taskByInsertionIndex.Add(task, i++);
            }

            var tasksInExecutionOrder = new Queue<IChangeLogTask>();  // the resulting list of tasks in the order in which they need to be executed
            var dependentTasks = new Stack<IChangeLogTask>();         // the stack of tasks that depend on the current task (required for detecting circular dependencies)

            //Adds the specified task and its dependencies to the queue
            void AddTask(IChangeLogTask task)
            {
                if (dependentTasks.Contains(task))
                {
                    dependentTasks.Push(task);
                    var circularDependency = String.Join(" -> ", dependentTasks.Reverse().Select(x => $"'{GetTaskName(x)}'"));
                    throw new InvalidPipelineConfigurationException($"Detected circular dependency between tasks: {circularDependency}");
                }

                // Task is already in result => nothing to do
                if (tasksInExecutionOrder.Contains(task))
                    return;

                dependentTasks.Push(task);
                // Add all dependencies to *before* the current task
                foreach (var (_, dependency) in dependencyGraph.GetOutgoingEdges(task).OrderBy(x => taskByInsertionIndex[x.To]))
                {
                    m_Logger.LogDebug($"Task '{GetTaskName(task)}' depends on task '{GetTaskName(dependency)}'");
                    AddTask(dependency);
                }
                dependentTasks.Pop();

                // Add current task
                tasksInExecutionOrder.Enqueue(task);
            }

            // Add all tasks to the result
            foreach (var task in m_Tasks)
            {
                AddTask(task);
            }

            return tasksInExecutionOrder;
        }

        private string GetTaskName(IChangeLogTask task) => task.GetType().Name;
    }
}
