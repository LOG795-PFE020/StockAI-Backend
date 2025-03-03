using System.Reflection;
using System.Security.Claims;
using Application.Commands.Interfaces;
using Application.Commands.Seedwork;
using Application.Common.Configurations;
using Application.Common.Interfaces;
using Application.Queries.Interfaces;
using Application.Queries.Seedwork;
using Configuration.Dispatchers;
using Configuration.Encryption;
using Domain.User;
using Infrastructure.RabbitMQ.Registration;
using Infrastructure.RabbitMQ.Services;
using Infrastructure.Repositories;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using Presentation.Api.Controllers;
using Presentation.Api.Middlewares;
using Presentation.Consumers;
using Presentation.Consumers.Messages;
using Presentation.Jobs;

namespace Configuration;

public sealed class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.Configure<PasswordSettings>(_configuration.GetSection(nameof(PasswordSettings)));
        services.Configure<DefaultAdmin>(_configuration.GetSection($"Users:{nameof(DefaultAdmin)}"));
        services.Configure<DefaultClient>(_configuration.GetSection($"Users:{nameof(DefaultClient)}"));

        services.AddSingleton(serviceProvider => serviceProvider.GetRequiredService<IOptions<PasswordSettings>>().Value);

        RegisterConfiguration(services);
        RegisterInfrastructure(services);
        RegisterPresentation(services);
        RegisterApplication(services);

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseCors();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseMiddleware<ApiLoggingMiddleware>();
        app.UseMiddleware<TransactionMiddleware>();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }

    private void RegisterApplication(IServiceCollection collection)
    {
        ScrutorScanForType(collection, typeof(IQueryHandler<,>), assemblyNames: "Application.Queries");
        ScrutorScanForType(collection, typeof(ICommandHandler<>), assemblyNames: "Application.Commands");
    }

    private void RegisterPresentation(IServiceCollection collection)
    {
        collection.AddHostedService<AddDefaultDbRecords>();
        collection.AddHostedService<TickJob>();

        collection.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            }).PartManager.ApplicationParts.Add(new AssemblyPart(typeof(AuthController).Assembly));

        collection.AddCors(options =>
        {
            options.AddDefaultPolicy(
                corsPolicyBuilder =>
                {
                    corsPolicyBuilder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
        });

        collection
            .AddIdentity<UserPrincipal, IdentityRole>(options =>
            {
                _configuration.Bind("PasswordSettings", options.Password);

                options.SignIn.RequireConfirmedAccount = false;

                // Disable specific default validations
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 1;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequiredUniqueChars = 0;
            })
            .AddEntityFrameworkStores<UserPrincipalContext>()
            .AddPasswordValidator<PasswordValidator>();


        collection.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = RsaKeyStorage.Instance.RsaSecurityKey,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                RoleClaimType = ClaimTypes.Role,
                ValidIssuer = "auth",
                ValidAudience = "pfe"
            };
        });
    }

    private void RegisterInfrastructure(IServiceCollection collection)
    {
        collection.AddScoped<IMigrateUserContext, UserPrincipalContext>(provider => provider.GetRequiredService<UserPrincipalContext>());
        collection.AddScoped<ITransactionInfo, TransactionInfo>();
        collection.AddScoped<IMessagePublisher, MessagePublisher>(provider =>
            new MessagePublisher(
                _configuration.GetConnectionString("Rabbitmq") ?? throw new InvalidOperationException("Rabbitmq connection string is not found"), 
                provider.GetRequiredService<ISendEndpointProvider>(),
                provider.GetRequiredService<ITransactionInfo>()));

        collection.AddDbContext<UserPrincipalContext>(RepositoryDbContextOptionConfiguration);

        collection.AddSingleton<IMongoClient>(_ => new MongoClient(_configuration.GetConnectionString("Mongodb")));
        collection.AddSingleton(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            return client.GetDatabase("Stocks");
        });

        collection.AddScoped<ISharesRepository, MongoSharesRepository>();

        collection.AddSingleton(typeof(IInMemoryStore<>), typeof(InMemoryStore<>));

        collection.RegisterMassTransit(
            _configuration.GetConnectionString("Rabbitmq") ??
            throw new InvalidOperationException("Rabbitmq connection string is not found"),
            new MassTransitConfigurator()
                .AddConsumer<StockQuote, QuoteConsumer>("quote-exchange", sp => new QuoteConsumer(sp.GetRequiredService<ICommandDispatcher>())));

        return;

        void RepositoryDbContextOptionConfiguration(DbContextOptionsBuilder options)
        {
            var connectionString = _configuration.GetConnectionString("Postgres");

            options.EnableThreadSafetyChecks();
            options.UseNpgsql(connectionString);
        }
    }

    private void RegisterConfiguration(IServiceCollection collection)
    {
        collection.AddSingleton<IRsaKeyStorage, RsaKeyStorage>(_ => RsaKeyStorage.Instance);
        collection.AddScoped<IQueryDispatcher, QueryDispatcher>();
        collection.AddScoped<ICommandDispatcher, CommandDispatcher>();
    }

    private void ScrutorScanForType(IServiceCollection services, Type type,
        ServiceLifetime lifetime = ServiceLifetime.Scoped, params string[] assemblyNames)
    {
        services.Scan(selector =>
        {
            selector.FromAssemblies(assemblyNames.Select(Assembly.Load))
                .AddClasses(filter => filter.AssignableTo(type))
                .AsImplementedInterfaces()
                .WithLifetime(lifetime);
        });
    }
}