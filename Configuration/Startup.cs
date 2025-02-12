using System.Reflection;
using System.Security.Claims;
using Application.Commands.Interfaces;
using Application.Commands.Seedwork;
using Application.Common.Configurations;
using Application.Queries.Interfaces;
using Application.Queries.Seedwork;
using Configuration.Dispatchers;
using Configuration.Encryption;
using Domain.User;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Presentation.Api.Controllers;
using Presentation.Api.Middlewares;
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
        var symmetricKeyProvider = new SymmetricKeyProvider();

        services.Configure<PasswordSettings>(_configuration.GetSection(nameof(PasswordSettings)));
        services.Configure<DefaultAdmin>(_configuration.GetSection($"Users:{nameof(DefaultAdmin)}"));
        services.Configure<DefaultClient>(_configuration.GetSection($"Users:{nameof(DefaultClient)}"));

        services.AddSingleton(serviceProvider => serviceProvider.GetRequiredService<IOptions<PasswordSettings>>().Value);

        RegisterConfiguration(services, symmetricKeyProvider);
        RegisterInfrastructure(services);
        RegisterPresentation(services, symmetricKeyProvider);
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

    private void RegisterPresentation(IServiceCollection collection, SymmetricKeyProvider symmetricKeyProvider)
    {
        collection.AddHostedService<AddDefaultDbRecords>();

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
                IssuerSigningKey = symmetricKeyProvider.SigningKey,
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                RoleClaimType = ClaimTypes.Role
            };
        });
    }

    private void RegisterInfrastructure(IServiceCollection collection)
    {
        collection.AddScoped<IMigrateUserContext, UserPrincipalContext>(provider => provider.GetRequiredService<UserPrincipalContext>());

        collection.AddDbContext<UserPrincipalContext>(RepositoryDbContextOptionConfiguration);

        return;

        void RepositoryDbContextOptionConfiguration(DbContextOptionsBuilder options)
        {
            var connectionString = _configuration.GetConnectionString("Postgres");

            options.EnableThreadSafetyChecks();
            options.UseNpgsql(connectionString);
        }
    }

    private void RegisterConfiguration(IServiceCollection collection, SymmetricKeyProvider symmetricKeyProvider)
    {
        collection.AddSingleton<ISymmetricKeyProvider, SymmetricKeyProvider>(_ => symmetricKeyProvider);
        collection.AddSingleton<IRsaKeyStorage, RsaKeyStorage>();
        collection.AddSingleton<IQueryDispatcher, QueryDispatcher>();
        collection.AddSingleton<ICommandDispatcher, CommandDispatcher>();
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