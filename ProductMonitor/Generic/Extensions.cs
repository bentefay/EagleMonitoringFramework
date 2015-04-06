using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductMonitor.Generic
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
