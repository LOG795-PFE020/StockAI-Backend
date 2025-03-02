namespace AuthServer.IntegrationTests.Tests.Rabbitmq.Messages.Impl;

public sealed class TestMessage : ITestMessage
{
    public Guid CorrelationId { get; set; }

    public string Message { get; set; }
}