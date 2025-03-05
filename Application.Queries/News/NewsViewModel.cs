using Domain.News;
using Domain.News.ValueObjects;

namespace Application.Queries.News;

public sealed class NewsViewModel
{
    public int GeneralOpinion { get; init; }

    public  double OpinionSkew { get; init; }

    public List<ArticleViewModel> ArticleViewModels { get; init; }

    public NewsViewModel(){}

    public NewsViewModel(List<Article> articles)
    {
        GeneralOpinion = articles.Select(a => a.Opinion).Aggregate(Opinion.WithNeutralOpinion(), (acc, opinion) => acc.Combine(opinion)).Value;

        OpinionSkew = articles.Select(a => a.Opinion.Value).Average();

        ArticleViewModels = articles.Select(a => new ArticleViewModel(a.Id, a.Title, a.PublishedAt, a.Opinion.Value)).ToList();
    }

    public sealed record ArticleViewModel(string Id, string Title, DateTime PublishedAt, int Opinion);
}