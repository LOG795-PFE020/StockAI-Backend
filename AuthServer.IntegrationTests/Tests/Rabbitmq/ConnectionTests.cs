using AuthServer.IntegrationTests.Infrastructure;
using AuthServer.IntegrationTests.Tests.Rabbitmq.Framework;
using AuthServer.IntegrationTests.Tests.Rabbitmq.Messages.Impl;
using FluentAssertions;

namespace AuthServer.IntegrationTests.Tests.Rabbitmq;

[Collection(nameof(TestCollections.Default))]
public class ConnectionTests
{
    private readonly ApplicationFactoryFixture _applicationFactoryFixture;

    public ConnectionTests(ApplicationFactoryFixture applicationFactoryFixture)
    {
        _applicationFactoryFixture = applicationFactoryFixture;
    }

    [Fact]
    public async Task WithTestExchangeEndpoint_SendMessage_ShouldReturnCorrectMessage()
    {
        const int timeout = 10_000;

        const string message = "Hello, World!";

        Guid correlationId = Guid.NewGuid();

        var sendEndpoint = _applicationFactoryFixture.WithMessagePublisher();

        await sendEndpoint.Publish(new TestMessage()
        {
            CorrelationId = correlationId.ToString(),
            Message = message
        });

         var responseMessage = await MessageSink.ListenFor<TestMessage>(correlationId.ToString(), new CancellationTokenSource(timeout).Token);

        responseMessage.Should().NotBeNull();
        responseMessage.Single().Message.Should().Be(message);
    }
}