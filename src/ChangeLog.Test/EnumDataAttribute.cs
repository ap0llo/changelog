using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit.Sdk;

namespace Grynwald.ChangeLog.Test
{
    public class EnumDataAttribute : DataAttribute
    {
        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            var parameters = testMethod.GetParameters();
            if (parameters.Length == 1 && parameters[0].ParameterType.IsEnum)
            {
                foreach (var value in Enum.GetValues(parameters[0].ParameterType))
                {
                    yield return new object[] { value! };
                }
            }
        }
    }
}
