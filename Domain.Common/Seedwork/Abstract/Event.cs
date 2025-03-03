namespace Domain.Common.Seedwork.Abstract;

public abstract class Event
{
    public string? CorrelationId { get; set; }
}