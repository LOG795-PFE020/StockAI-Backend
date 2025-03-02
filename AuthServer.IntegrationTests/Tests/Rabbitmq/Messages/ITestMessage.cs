namespace AuthServer.IntegrationTests.Tests.Rabbitmq.Messages;

public interface ITestMessage
{
    Guid CorrelationId { get; set; }
}