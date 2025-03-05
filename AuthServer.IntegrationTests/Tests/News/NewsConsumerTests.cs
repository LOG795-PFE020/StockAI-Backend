using Application.Commands.Interfaces;
using Application.Common.Interfaces;
using AuthServer.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace AuthServer.IntegrationTests.Tests.News;

[Collection(nameof(TestCollections.Default))]
public sealed class NewsConsumerTests
{
    private readonly ApplicationFactoryFixture _applicationFactoryFixture;

    public NewsConsumerTests(ApplicationFactoryFixture applicationFactoryFixture)
    {
        _applicationFactoryFixture = applicationFactoryFixture;
    }

    [Fact]
    public async Task WithValidNews_NewsConsumer_ShouldIndexNewsSuccessfully()
    {
        const string symbol = "AAPL";

        DateTime date = DateTime.UtcNow;

        const string content = "Today we saw...";

        var news = new Presentation.Consumers.Messages.News
        {
            Title = "Apple Inc.",
            Symbol = symbol,
            Content = content,
            PublishedAt = date,
            Opinion = 1
        };

        await _applicationFactoryFixture.WithMessagePublished(news);

        using var scope = _applicationFactoryFixture.Services.CreateScope();

        IArticleRepository newsRepository = scope.ServiceProvider.GetRequiredService<IArticleRepository>();
        IAzureBlobRepository azureBlobRepository = scope.ServiceProvider.GetRequiredService<IAzureBlobRepository>();

        var article = await newsRepository.GetBySymbolAsync(news.Symbol);

        article.Should().NotBeNull();

        article.PublishedAt.Date.Should().Be(date.Date);

        var blob = await azureBlobRepository.DownloadBlobAsync(article.ContentId);

        blob.Should().Be(content);
    }
}