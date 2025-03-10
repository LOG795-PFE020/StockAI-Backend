using Application.Commands.Interfaces;
using Application.Commands.Seedwork;
using Application.Common.Interfaces;
using Domain.Common.Monads;
using Domain.News;

namespace Application.Commands.News;

public sealed class IndexArticleHandler : ICommandHandler<IndexArticle>
{
    private readonly IArticleRepository _articleRepository;
    private readonly IAzureBlobRepository _blobRepository;

    private static readonly SemaphoreSlim SemaphoreSlim = new(1);

    public IndexArticleHandler(IArticleRepository articleRepository, IAzureBlobRepository blobRepository)
    {
        _articleRepository = articleRepository;
        _blobRepository = blobRepository;
    }

    public async Task<Result> Handle(IndexArticle command, CancellationToken cancellation)
    {
        await SemaphoreSlim.WaitAsync(cancellation);

        try
        {
            long articleCountForSymbol = await _articleRepository.GetNumberOfArticleForSymbol(command.SymbolId);

            string contentId = $"{command.SymbolId}-{articleCountForSymbol + 1}";

            var article = new Article(Guid.NewGuid().ToString(), command.Opinion)
            {
                Title = command.Title,
                ContentId = contentId,
                PublishedAt = command.PublishedAt,
                SymbolId = command.SymbolId
            };

            await _articleRepository.AddAsync(article);

            await _blobRepository.UploadBlobAsync(contentId, command.Content);

            return Result.Success();
        }
        finally
        {
            SemaphoreSlim.Release();
        }
    }
}