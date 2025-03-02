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

        var sendEndpoint = await _applicationFactoryFixture.WithTestExchangeEndpoint();

        await sendEndpoint.Send(new TestMessage()
        {
            CorrelationId = correlationId,
            Message = message
        });

         var responseMessage = await MessageSink.ListenFor<TestMessage>(correlationId, new CancellationTokenSource(timeout).Token);

        responseMessage.Should().NotBeNull();
        responseMessage.Message.Should().Be(message);
    }
}