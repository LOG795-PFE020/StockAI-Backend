using Application.Commands.Interfaces;
using Application.Common.Interfaces;
using AuthServer.IntegrationTests.Infrastructure;
using AuthServer.IntegrationTests.Tests.News.Services;
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

        await NewsPublisher.PublishAsync(_applicationFactoryFixture, content);

        using var scope = _applicationFactoryFixture.Services.CreateScope();

        IArticleRepository newsRepository = scope.ServiceProvider.GetRequiredService<IArticleRepository>();
        IAzureBlobRepository azureBlobRepository = scope.ServiceProvider.GetRequiredService<IAzureBlobRepository>();

        var article = await newsRepository.GetBySymbolAsync(symbol);

        article.Should().NotBeNull();

        article.PublishedAt.Date.Should().Be(date.Date);

        var blob = await azureBlobRepository.DownloadBlobAsync(article.ContentId);

        blob.Should().Be(content);
    }
}