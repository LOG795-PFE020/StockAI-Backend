using System.Collections.Concurrent;
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
using Application.Common.Interfaces;
using AuthServer.IntegrationTests.Tests.Rabbitmq.Consumers;
using Microsoft.Extensions.Options;
using Infrastructure.RabbitMQ.Registration;
using AuthServer.IntegrationTests.Tests.Rabbitmq.Messages.Impl;
using Domain.Time.DomainEvents;
using AuthServer.IntegrationTests.Tests.Time.Consumers;
using MassTransit;
using Presentation.Consumers;
using Presentation.Consumers.Messages;
using Microsoft.AspNetCore.TestHost;
using Event = Domain.Common.Seedwork.Abstract.Event;

namespace AuthServer.IntegrationTests.Infrastructure;

public sealed class ApplicationFactoryFixture : WebApplicationFactory<Startup>, IAsyncLifetime
{
    public IDictionary<Guid, TaskCompletionSource<Event>> TransactionCompletedNotifier => s_transactionCompletedNotifier;

    private static readonly ConcurrentDictionary<Guid, TaskCompletionSource<Event>> s_transactionCompletedNotifier = new();

    private readonly Postgres _postgres = new();
    private readonly Rabbitmq _rabbitmq = new();
    private readonly Mongodb _mongodb = new();
    private readonly Azurite _azurite = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureAppConfiguration((_, config) =>
        {
            var integrationConfig = new Dictionary<string, string>
            {
                ["ConnectionStrings:Postgres"] = _postgres.Container.GetConnectionString(),
                ["ConnectionStrings:Rabbitmq"] = _rabbitmq.Container.GetConnectionString(),
                ["ConnectionStrings:Mongodb"] = _mongodb.Container.GetConnectionString(),
                ["ConnectionStrings:Blob"] = _azurite.Container.GetConnectionString(),
            };
            
            config.AddInMemoryCollection(integrationConfig!);
        });

        builder.ConfigureTestServices(services =>
        {
            // Remove any existing MassTransit registrations to prevent duplicate configuration.
            var massTransitDescriptors = services
                .Where(s => s.ServiceType?.Namespace?.Contains("MassTransit") == true)
                .ToList();

            foreach (var descriptor in massTransitDescriptors)
            {
                services.Remove(descriptor);
            }

            services.RegisterMassTransit(
                _rabbitmq.Container.GetConnectionString(),
                new MassTransitConfigurator()
                    .AddPublisher<TestMessage>("test-exchange")
                    .AddConsumer<TestMessage, ConsumerDecorator<TestMessage, TestMessageConsumer>>("test-exchange", _ => new(new()))
                    .AddPublisher<DayStarted>("day-started-exchange")
                    .AddConsumer<DayStarted, TestTimeMessageConsumer>("day-started-exchange")
                    .AddPublisher<StockQuote>("quote-exchange")
                    .AddConsumer<StockQuote, ConsumerDecorator<StockQuote, QuoteConsumer>>("quote-exchange", sp =>
                    {
                        var scope = sp.CreateScope();
                        return new (new QuoteConsumer(scope.ServiceProvider.GetRequiredService<ICommandDispatcher>()));
                    })
                    .AddPublisher<News>("news-exchange")
                    .AddConsumer<News, ConsumerDecorator<News, NewsConsumer>> ("news-exchange", sp =>
                    {
                        var scope = sp.CreateScope();
                        return new ( new(scope.ServiceProvider.GetRequiredService<ICommandDispatcher>()));
                    }));
        });
    }

    public async Task<TMessage> WithMessagePublished<TMessage>(TMessage message, Guid correlationId = default) where TMessage : Event
    {
        using var scope = Services.CreateScope();

        var transactionInfo = scope.ServiceProvider.GetRequiredService<ITransactionInfo>();

        transactionInfo.CorrelationId = correlationId = correlationId == default ? Guid.NewGuid() : correlationId;

        var tcs = new TaskCompletionSource<Event>();

        s_transactionCompletedNotifier.TryAdd(correlationId, tcs);

        var messagePublisher = scope.ServiceProvider.GetRequiredService<IMessagePublisher>();

        await messagePublisher.Publish(message);

        return (TMessage)await tcs.Task;
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
        await _mongodb.InitializeAsync();
        await _azurite.InitializeAsync();
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();

        await _postgres.DisposeAsync();
        await _rabbitmq.DisposeAsync();
        await _mongodb.DisposeAsync();
        await _azurite.DisposeAsync();
    }

    public string? GetRoleFromJwt(string jwt)
    {
        var handler = new JwtSecurityTokenHandler();

        var tokenS = (JwtSecurityToken)handler.ReadToken(jwt);

        var roleClaim = tokenS.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Role);

        return roleClaim?.Value;
    }

    private sealed class ConsumerDecorator<TConsumed, TDecorated>(TDecorated decorated) : IConsumer<TConsumed>
        where TConsumed : Event
        where TDecorated : class, IConsumer<TConsumed>
    {
        public async Task Consume(ConsumeContext<TConsumed> context)
        {
            await decorated.Consume(context);

            if (context.CorrelationId is { } span && s_transactionCompletedNotifier.TryGetValue(span, out var value))
            {
                value.SetResult(context.Message);                s_transactionCompletedNotifier.TryRemove(span, out _);
            }
        }
    }
}