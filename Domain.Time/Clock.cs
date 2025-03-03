using Domain.Common.Seedwork.Abstract;
using Domain.Time.DomainEvents;

namespace Domain.Time;

public sealed class Clock : Aggregate<Clock>
{
    private DateTime _latestTickRealTime = DateTime.UtcNow;

    private DateTime _syntheticDateTime = DateTime.UtcNow;

    private DateTime _latestDayStarted = DateTime.UtcNow.Date;

    private int _multiplier = 1;

    private readonly object _lock = new();

    public Clock() : base(Guid.NewGuid().ToString()) { }

    public void SetMultiplier(int multiplier)
    {
        if(multiplier <= 1)
        {
            throw new ArgumentException("Multiplier must be greater than 1");
        }

        if(multiplier > 86400)
        {
            throw new ArgumentException("Multiplier must be less than 8 640");
        }

        _multiplier = multiplier;
    }

    public void Tick()
    {
        lock (_lock)
        {
            // Calculate the time that has passed since the last tick
            TimeSpan delta = (DateTime.UtcNow - _latestTickRealTime);

            // Update the latest tick real time
            _latestTickRealTime = DateTime.UtcNow;

            // Calculate the new synthetic date time
            _syntheticDateTime = _syntheticDateTime.Add(delta * _multiplier);

            // Calculate the number of days that have passed
            var deltaDays = (int)(_syntheticDateTime.Date - _latestDayStarted).TotalDays;

            for (int i = 0; i < deltaDays; i++)
            {
                AddDomainEvent(new DayStarted(_syntheticDateTime.Date.AddDays(i)));

                _latestDayStarted = _syntheticDateTime.Date.AddDays(i);
            }
        }
    }
}