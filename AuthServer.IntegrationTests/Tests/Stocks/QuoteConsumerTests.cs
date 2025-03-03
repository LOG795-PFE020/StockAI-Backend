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
    public async Task QuoteConsumer_ShouldUpdateShare()
    {
        const string symbol = "AAPL";
        const decimal price = 100;
        
        DateTime date = DateTime.UtcNow;

        Guid correlationId = Guid.NewGuid();

        var quote = new StockQuote
        {
            Symbol = symbol,
            Price = price,
            Date = date,
            CorrelationId = correlationId.ToString()
        };

        IMessagePublisher messagePublisher = _applicationFactoryFixture.WithMessagePublisher();

        await messagePublisher.Publish(quote);

        //Could use a notification instead
        await Task.Delay(5_000);

        using var scope = _applicationFactoryFixture.Services.CreateScope();

        ISharesRepository sharesRepository = scope.ServiceProvider.GetRequiredService<ISharesRepository>();

        var share = await sharesRepository.GetBySymbolAsync(quote.Symbol);

        share.Should().NotBeNull();

        share.GetPrice(date.Date).Content.Should().Be(price);
    }
}