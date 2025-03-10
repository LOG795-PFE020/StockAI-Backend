using DotNet.Testcontainers.Builders;
using Testcontainers.Azurite;

namespace AuthServer.IntegrationTests.Infrastructure.TestContainer;

internal sealed class Azurite
{
    internal AzuriteContainer Container { get; } = new AzuriteBuilder()
        .WithImage("mcr.microsoft.com/azure-storage/azurite:latest")
        .WithPortBinding(10000, 10000)
        .WithPortBinding(10001, true)
        .WithPortBinding(10002, true)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(10000))
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