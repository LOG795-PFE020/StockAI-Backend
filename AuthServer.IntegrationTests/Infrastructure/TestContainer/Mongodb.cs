using DotNet.Testcontainers.Builders;
using Testcontainers.MongoDb;

namespace AuthServer.IntegrationTests.Infrastructure.TestContainer;

internal sealed class Mongodb
{
    public MongoDbContainer Container { get; } = new MongoDbBuilder()
        .WithImage("mongo:8.0.5")
        .WithPortBinding(27017, 27017)
        .WithUsername("mongo")
        .WithPassword("mongo")
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(27017))
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