using Application.Commands.Quotes;
using Application.Commands.Seedwork;
using MassTransit;
using Presentation.Consumers.Messages;

namespace Presentation.Consumers;

public sealed class QuoteConsumer : IConsumer<StockQuote>
{
    private readonly ICommandDispatcher _commandDispatcher;

    public QuoteConsumer(ICommandDispatcher commandDispatcher)
    {
        _commandDispatcher = commandDispatcher;
    }

    public async Task Consume(ConsumeContext<StockQuote> context)
    {
        var quote = context.Message;

        await _commandDispatcher.DispatchAsync(new AddQuote(quote.Symbol, quote.Date, quote.Price));
    }
}