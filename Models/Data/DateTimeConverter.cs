using System;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HRwflow.Models.Data
{
    public class DateTimeConverter : ValueConverter<DateTime, long>
    {
        public DateTimeConverter() : base(dateTime => FromDateTime(dateTime),
                                 timestamp => ToDateTime(timestamp))
        { }

        public static long FromDateTime(DateTime dateTime)
            => new DateTimeOffset(dateTime).ToUnixTimeSeconds();

        public static DateTime ToDateTime(long timestamp)
                    => DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime;
    }
}
