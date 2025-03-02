using AuthServer.IntegrationTests.Tests.Rabbitmq.Framework;
using AuthServer.IntegrationTests.Tests.Rabbitmq.Messages.Impl;
using MassTransit;

namespace AuthServer.IntegrationTests.Tests.Rabbitmq.Consumers;

public sealed class TestMessageConsumer : IConsumer<TestMessage>
{
    public Task Consume(ConsumeContext<TestMessage> context)
    {
        MessageSink.AddMessage(context.Message);

        return Task.CompletedTask;
    }
}