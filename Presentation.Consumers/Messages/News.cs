using Domain.Common.Seedwork.Abstract;

namespace Presentation.Consumers.Messages;

public sealed class News : Event
{
    public string Title { get; init; }
    public string Symbol { get; init; }
    public string Content { get; init; }
    public DateTime PublishedAt { get; init; }
    public int Opinion { get; init; }
}