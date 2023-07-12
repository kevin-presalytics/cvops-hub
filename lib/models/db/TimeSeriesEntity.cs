using System;

namespace lib.models.db
{
    public interface ITimeSeriesEntity
    {
        DateTimeOffset Time {get; set;}
    }

    public abstract class TimeSeriesEntity : EventArgs, ITimeSeriesEntity
    {
    public DateTimeOffset Time {get; set;} = DateTimeOffset.UtcNow;
    }
}