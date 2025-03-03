using AuthServer.IntegrationTests.Tests.Rabbitmq.Framework;
using Domain.Time.DomainEvents;
using MassTransit;

namespace AuthServer.IntegrationTests.Tests.Time.Consumers;

public sealed class TestTimeMessageConsumer : IConsumer<DayStarted>
{
    public Task Consume(ConsumeContext<DayStarted> context)
    {
        MessageSink.AddMessage(context.Message);

        return Task.CompletedTask;
    }
}