using System;

namespace ChangeLogCreator.Test
{
    internal sealed class DateTimeSource
    {
        public DateTime Current { get; private set; } = new DateTime(2020, 1, 1);


        public DateTime Next()
        {
            var date = Current;
            Current = Current.AddDays(1);
            return date;
        }

    }
}
