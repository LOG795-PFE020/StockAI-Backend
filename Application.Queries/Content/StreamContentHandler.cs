using Application.Common.Interfaces;
using Application.Queries.Seedwork;
using Domain.Common.Monads;
using Domain.News;
using MongoDB.Driver;

namespace Application.Queries.Content;

public sealed class StreamContentHandler : IQueryHandler<StreamContent, Stream>
{
    private readonly IAzureBlobRepository _azureBlobRepository;
    private readonly IMongoCollection<Article> _articles;

    public StreamContentHandler(IMongoClient client, IAzureBlobRepository azureBlobRepository)
    {
        _azureBlobRepository = azureBlobRepository;
        var database = client.GetDatabase("News");
        _articles = database.GetCollection<Article>("Articles");
    }


    public async Task<Result<Stream>> Handle(StreamContent query, CancellationToken cancellation)
    {
        var article = _articles.Find(a => a.Id == query.ArticleId).FirstOrDefault(cancellation);

        if (article is null)
        {
            return Result.Failure<Stream>($"Article with id '{query.ArticleId}' not found");
        }

        return Result.Success(await _azureBlobRepository.GetAsStream(article.ContentId));
    }
}