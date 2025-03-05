using Application.Commands.Interfaces;
using AuthServer.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Presentation.Consumers.Messages;

namespace AuthServer.IntegrationTests.Tests.Stocks;

[Collection(nameof(TestCollections.Default))]
public sealed class QuoteConsumerTests
{
    private readonly ApplicationFactoryFixture _applicationFactoryFixture;

    public QuoteConsumerTests(ApplicationFactoryFixture applicationFactoryFixture)
    {
        _applicationFactoryFixture = applicationFactoryFixture;
    }

    [Fact]
    public async Task WithValidStockQuote_QuoteConsumer_ShouldIndexShareSuccessfully()
    {
        const string symbol = "AAPL";
        const decimal price = 100;
        
        DateTime date = DateTime.UtcNow;

        var quote = new StockQuote
        {
            Symbol = symbol,
            Price = price,
            Date = date,
        };

        await _applicationFactoryFixture.WithMessagePublished(quote);

        using var scope = _applicationFactoryFixture.Services.CreateScope();

        ISharesRepository sharesRepository = scope.ServiceProvider.GetRequiredService<ISharesRepository>();

        var share = await sharesRepository.GetBySymbolAsync(quote.Symbol);

        share.Should().NotBeNull();

        share.GetPrice(date.Date).Content.Should().Be(price);
    }
}