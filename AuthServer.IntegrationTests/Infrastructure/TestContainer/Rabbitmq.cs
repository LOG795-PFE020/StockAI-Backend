using Testcontainers.RabbitMq;

namespace AuthServer.IntegrationTests.Infrastructure.TestContainer;

internal sealed class Rabbitmq : IAsyncLifetime
{
    public RabbitMqContainer Container { get; } = new RabbitMqBuilder()
        .WithImage("rabbitmq:3.13.7-management")
        .WithHostname("rabbitmq")
        .WithExposedPort(5672)
        .WithPortBinding(15672, true)
        .WithUsername("guest")
        .WithPassword("guest")
        .Build();

    public Task InitializeAsync()
    {
        return Container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await Container.DisposeAsync();
    }
}