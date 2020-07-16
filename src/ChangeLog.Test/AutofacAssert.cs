using Autofac;
using Autofac.Core;
using Xunit.Sdk;

namespace Grynwald.ChangeLog.Test
{
    internal static class AutofacAssert
    {
        public static void CanResolveType<T>(IContainer container) where T : notnull
        {
            try
            {
                var instance = container.Resolve<T>();

                if (instance is null)
                {
                    throw new XunitException($"Failed to resolve type '{typeof(T).FullName}' from container but type was expected to be resolvable.");
                }
            }
            catch (DependencyResolutionException)
            {
                throw new XunitException($"Failed to resolve type '{typeof(T).FullName}' from container but type was expected to be resolvable.");
            }
        }
    }
}
