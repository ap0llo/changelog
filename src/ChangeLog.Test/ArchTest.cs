using System;
using System.Linq;
using Grynwald.ChangeLog.Pipeline;
using NetArchTest.Rules;
using Xunit;
using Xunit.Sdk;

namespace Grynwald.ChangeLog
{
    /// <summary>
    /// Rules that verify consistency rules of the implementation
    /// </summary>
    public class ArchTest
    {
        public class Implementation_of_IEquatable
        {
            [Fact]
            public void Implementations_of_IEquatable_are_sealed()
            {
                var result = Types.InAssembly(typeof(Program).Assembly)
                    .That().ImplementInterface(typeof(IEquatable<>))
                    .And().AreNotAbstract()
                    .Should().BeSealed()
                    .GetResult();

                if (!result.IsSuccessful)
                {
                    throw new XunitException(
                        "The following types should be sealed: \r\n" +
                        String.Join("\r\n", result.FailingTypeNames.Select(x => $" - {x}")));
                }
            }
        }


        public class Implementation_of_IChangeLogTask
        {
            [Theory]
            [InlineData(typeof(AfterTaskAttribute))]
            [InlineData(typeof(BeforeTaskAttribute))]
            public void Classes_with_a_task_dependency_attribute_implement_IChangeLogTask(Type attributeType)
            {
                // Check for implementation of the IChangeLogTask interface through reflection
                // because NetArchTest's Should().ImplementInterface() only seems to work if a type
                // implements the interface directory and not for cases where
                // the base class implements the interface
                var invalidTypes = Types.InAssembly(typeof(Program).Assembly)
                    .That().HaveCustomAttribute(attributeType)
                    .GetTypes()
                    .Where(t => !typeof(IChangeLogTask).IsAssignableFrom(t));

                if (invalidTypes.Any())
                {
                    throw new XunitException(
                        $"The following types have a [{attributeType.Name.RemoveSuffix("Attribute")}] attribute but do not implement IChangeLogTask: \r\n" +
                        String.Join("\r\n", invalidTypes.Select(x => $" - {x.Name}")));
                }
            }

            [Theory]
            [InlineData(typeof(AfterTaskAttribute))]
            [InlineData(typeof(BeforeTaskAttribute))]
            public void Dependencies_declared_using_a_task_dependency_attribute_must_implement_IChangeLogTask(Type attributeType)
            {
                // ARRANGE
                var result = Types.InAssembly(typeof(Program).Assembly)
                    .That().HaveCustomAttribute(attributeType)
                    .GetTypes();

                var invalidDependencies = result.SelectMany(type =>
                    type
                        .GetCustomAttributes(attributeType, false)
                        .Cast<TaskDependencyAttribute>()
                        .Select(x => x.TaskType)
                        .Where(x => !typeof(IChangeLogTask).IsAssignableFrom(x))
                        .Select(x => new { DeclaringType = type, DependencyType = x })
                );

                if (invalidDependencies.Any())
                {
                    throw new XunitException(
                        $"The following task dependency attributes are invalid because the defined dependency does not implement IChangeLogTask: \r\n" +
                        String.Join("\r\n", invalidDependencies.Select(x => $" - [{attributeType.Name.RemoveSuffix("Attribute")}({x.DependencyType})] on {x.DeclaringType}")));
                }

            }

            [Theory]
            [InlineData(typeof(AfterTaskAttribute))]
            [InlineData(typeof(BeforeTaskAttribute))]
            public void Dependencies_referenced_using_a_task_dependency_attribute_must_be_sealed(Type attributeType)
            {
                // Explanation:
                // ChangeLogPipeline will evaluate the [AfterTask] and [BeforeTask] attributes to determine the execution order of tasks.
                // To keep the implementation simple, ChangeLogPipeline will only look for exact matches of a task's type and the type referenced using the attribute.
                // Thus, all referenced types should be sealed so we can be sure there are no derived types which the pipeline would have to handle.

                var result = Types.InAssembly(typeof(Program).Assembly)
                    .That().HaveCustomAttribute(attributeType)
                    .GetTypes();

                var invalidDependencies = result.SelectMany(type =>
                    type
                        .GetCustomAttributes(attributeType, false)
                        .Cast<TaskDependencyAttribute>()
                        .Select(x => x.TaskType)
                        .Where(x => !x.IsSealed)
                ).Distinct();

                if (invalidDependencies.Any())
                {
                    throw new XunitException(
                        $"The following tasks are referenced using a [{attributeType.Name.RemoveSuffix("Attribute")}] attribute and should thus be sealed: \r\n" +
                        String.Join("\r\n", invalidDependencies.Select(x => $" - {x}")));
                }

            }
        }
    }
}
