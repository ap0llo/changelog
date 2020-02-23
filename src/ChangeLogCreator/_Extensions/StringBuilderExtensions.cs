using System;
using System.Collections.Generic;
using System.Text;

namespace ChangeLogCreator
{
    internal static class StringBuilderExtensions
    {
        public static string GetValueAndClear(this StringBuilder stringBuilder)
        {
            var value = stringBuilder.ToString();
            stringBuilder.Clear();
            return value;
        }
    }
}
