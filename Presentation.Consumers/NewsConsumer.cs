using Application.Commands.News;
using Application.Commands.Seedwork;
using MassTransit;
using Presentation.Consumers.Messages;

namespace Presentation.Consumers;

public class NewsConsumer : IConsumer<News>
{
    private readonly ICommandDispatcher _commandDispatcher;

    public NewsConsumer(ICommandDispatcher commandDispatcher)
    {
        _commandDispatcher = commandDispatcher;
    }

    public async Task Consume(ConsumeContext<News> context)
    {
        News news = context.Message;

        var command = new IndexArticle(news.Title, news.Symbol, news.Content, news.PublishedAt, news.Opinion);

        await _commandDispatcher.DispatchAsync(command);
    }
}