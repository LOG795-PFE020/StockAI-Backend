using AuthServer.IntegrationTests.Infrastructure;
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
        const string message = "Hello, World!";

        var responseMessage = await _applicationFactoryFixture.WithMessagePublished(new TestMessage()
        {
            Message = message
        });

        responseMessage.Should().NotBeNull();
        responseMessage.Message.Should().Be(message);
    }
}