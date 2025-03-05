using Domain.Common.Seedwork.Abstract;
using Domain.News.ValueObjects;

namespace Domain.News;

public sealed class Article : Aggregate<Article>
{
    public required string Title { get; init; }

    public required string SymbolId { get; init; }

    public required string ContentId { get; init; }

    public required DateTime PublishedAt { get; init; }

    public Opinion Opinion { get; init; }

    private Article(string id) : base(id) {}

    public Article(string id, int opinion) : base(id)
    {
        Opinion = opinion switch
        {
            1 => Opinion.WithPositiveOpinion(),
            0 => Opinion.WithNeutralOpinion(),
            -1 => Opinion.WithNegativeOpinion(),
            _ => throw new ArgumentOutOfRangeException(nameof(opinion))
        };
    }

}