using Application.Common.Dtos;
using AuthServer.IntegrationTests.Infrastructure.TestContainer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using FluentAssertions;
using Domain.User;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Application.Commands.AdminPassword;
using Application.Commands.Interfaces;
using Application.Commands.Seedwork;
using Configuration;
using Microsoft.Extensions.DependencyInjection;
using Presentation.Jobs;
using Application.Common.Configurations;
using AuthServer.IntegrationTests.Tests.Rabbitmq.Consumers;
using Microsoft.Extensions.Options;
using Infrastructure.RabbitMQ.Registration;
using AuthServer.IntegrationTests.Tests.Rabbitmq.Messages.Impl;
using Domain.Time.DomainEvents;
using AuthServer.IntegrationTests.Tests.Time.Consumers;

namespace AuthServer.IntegrationTests.Infrastructure;

public sealed class ApplicationFactoryFixture : WebApplicationFactory<Startup>, IAsyncLifetime
{
    private readonly Postgres _postgres = new();
    private readonly Rabbitmq _rabbitmq = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureAppConfiguration((_, config) =>
        {
            var integrationConfig = new Dictionary<string, string>
            {
                ["ConnectionStrings:Postgres"] = _postgres.Container.GetConnectionString(),
                ["ConnectionStrings:Rabbitmq"] = _rabbitmq.Container.GetConnectionString(),
            };
            
            config.AddInMemoryCollection(integrationConfig!);
        });

        builder.ConfigureServices(services =>
        {
            services.RegisterMassTransit(
                _rabbitmq.Container.GetConnectionString(),
        new MassTransitConfigurator()
                .AddPublisher<TestMessage>("test-exchange")
                .AddConsumer<TestMessage, TestMessageConsumer>("test-exchange")
                .AddPublisher<DayStarted>("day-started-exchange")
                .AddConsumer<DayStarted, TestTimeMessageConsumer>("day-started-exchange"));
        });
    }

    public IMessagePublisher WithMessagePublisher()
    {
        using var scope = Services.CreateScope();

        return scope.ServiceProvider.GetRequiredService<IMessagePublisher>();
    }

    public async Task<HttpClient> WithAdminAuthAsync(Guid testId = default)
    {
        using var scope = Services.CreateScope();

        var defaultAdmin = scope.ServiceProvider.GetRequiredService<IOptions<DefaultAdmin>>();

        var commandDispatcher = scope.ServiceProvider.GetRequiredService<ICommandDispatcher>();

        await ServiceReady.Instance.IsReady.Task;

        await commandDispatcher.DispatchAsync(new ResetUserPassword(defaultAdmin.Value.Username, defaultAdmin.Value.Password));

        var httpResponseMessage = await SigninAsync(new UserCredentials
        {
            Username = defaultAdmin.Value.Username,
            Password = defaultAdmin.Value.Password
        });

        var jwt = await httpResponseMessage.Content.ReadAsStringAsync();

        GetRoleFromJwt(jwt).Should().Be(RoleConstants.AdminRole);

        var adminClient = CreateDefaultClient();

        adminClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {jwt}");
        adminClient.DefaultRequestHeaders.Add("CorrelationId", testId.ToString());

        return adminClient;
    }

    public async Task<HttpClient> WithClientUserAuthAsync(Guid testId = default)
    {
        using var scope = Services.CreateScope();

        var userDefault = scope.ServiceProvider.GetRequiredService<IOptions<DefaultClient>>();

        var commandDispatcher = scope.ServiceProvider.GetRequiredService<ICommandDispatcher>();

        await ServiceReady.Instance.IsReady.Task;

        await commandDispatcher.DispatchAsync(new ResetUserPassword(userDefault.Value.Username, userDefault.Value.Password));

        var httpResponseMessage = await SigninAsync(new UserCredentials
        {
            Username = userDefault.Value.Username,
            Password = userDefault.Value.Password
        });

        var jwt = await httpResponseMessage.Content.ReadAsStringAsync();

        GetRoleFromJwt(jwt).Should().Be(RoleConstants.Client);

        var adminClient = CreateDefaultClient();

        adminClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {jwt}");
        adminClient.DefaultRequestHeaders.Add("CorrelationId", testId.ToString());

        return adminClient;
    }

    public async Task<HttpResponseMessage> SigninAsync(UserCredentials userCredentials)
    {
        var unauthorizedClient = CreateDefaultClient();

        // Fetch public key from the server
        var publicKeyResponse = await unauthorizedClient.GetFromJsonAsync<ServerPublicKey>("auth/publickey");

        string pemPublicKey = publicKeyResponse.PublicKey;

        using var rsa = RSA.Create();

        rsa.ImportFromPem(pemPublicKey.ToCharArray());  // Import public key

        // Serialize and encrypt the credentials
        string serializedCredentials = JsonConvert.SerializeObject(userCredentials);
        byte[] credentialsBytes = Encoding.UTF8.GetBytes(serializedCredentials);
        byte[] encryptedData = rsa.Encrypt(credentialsBytes, RSAEncryptionPadding.OaepSHA256);

        var encryptedCredentials = new EncryptedCredentials
        {
            EncryptedData = Convert.ToBase64String(encryptedData)
        };

        // Post encrypted credentials to the server
        var response = await unauthorizedClient.PostAsJsonAsync("auth/signin", encryptedCredentials);

        return response;
    }

    public async Task InitializeAsync()
    {
        await _postgres.InitializeAsync();
        await _rabbitmq.InitializeAsync();
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();

        await _postgres.DisposeAsync();
        await _rabbitmq.DisposeAsync();
    }

    public string? GetRoleFromJwt(string jwt)
    {
        var handler = new JwtSecurityTokenHandler();

        var tokenS = (JwtSecurityToken)handler.ReadToken(jwt);

        var roleClaim = tokenS.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Role);

        return roleClaim?.Value;
    }
}