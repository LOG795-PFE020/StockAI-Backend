using Domain.Common.Seedwork.Abstract;

namespace Domain.Time.DomainEvents;

public sealed class DayStarted : Event
{
    public DateTime NewDay { get; }

    public DayStarted(DateTime newDay)
    {
        NewDay = newDay;
    }
}