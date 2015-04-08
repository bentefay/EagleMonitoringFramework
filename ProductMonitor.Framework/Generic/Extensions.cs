using System;

namespace ProductMonitor.Framework.Generic
{
    public static class DateTimeOffsetExtensions
    {
        public static DateTimeOffset ToNemOffset(this DateTimeOffset dateTimeOffset)
        {
            return dateTimeOffset.ToOffset(NemInterval.NemTimeOffset);
        }
    }

    public static class DateTimeExtensions
    {
        public static DateTimeOffset ToNemOffset(this DateTime dateTimeOffset)
        {
            return new DateTimeOffset(dateTimeOffset, NemInterval.NemTimeOffset);
        }
    }

    public static class NemInterval
    {
        public static TimeSpan NemTimeOffset = TimeSpan.FromHours(10);
    }
}
