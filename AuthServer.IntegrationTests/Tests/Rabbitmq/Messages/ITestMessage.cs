namespace AuthServer.IntegrationTests.Tests.Rabbitmq.Messages;

public interface ITestMessage
{
    string CorrelationId { get; set; }
}