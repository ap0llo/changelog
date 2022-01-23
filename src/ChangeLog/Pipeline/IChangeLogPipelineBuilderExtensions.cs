namespace Grynwald.ChangeLog.Pipeline
{
    internal static class IChangeLogPipelineBuilderExtensions
    {
        public static IChangeLogPipelineBuilder AddTaskIf<T>(this IChangeLogPipelineBuilder builder, bool condition) where T : IChangeLogTask
        {
            if (condition)
            {
                builder.AddTask<T>();
            }

            return builder;
        }
    }

}
