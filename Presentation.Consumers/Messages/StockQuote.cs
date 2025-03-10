using Domain.Common.Seedwork.Abstract;

namespace Presentation.Consumers.Messages;

public sealed class StockQuote : Event
{
    public string Symbol { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public DateTime Date { get; init; }
}