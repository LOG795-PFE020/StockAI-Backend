using DotNet.Testcontainers.Builders;
using Testcontainers.PostgreSql;

namespace AuthServer.IntegrationTests.Infrastructure.TestContainer;

public sealed class Postgres : IAsyncLifetime
{
    public PostgreSqlContainer Container { get; } = new PostgreSqlBuilder()
        .WithImage("postgres:latest")
        .WithPortBinding(5432, true)
        .WithDatabase("auth")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
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