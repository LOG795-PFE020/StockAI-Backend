using Application.Common.Configurations;
using Application.Common.Dtos;
using AuthServer.IntegrationTests.Infrastructure;
using System.Net.Http.Json;
using System.Net;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AuthServer.IntegrationTests.Tests.Auth;

[Collection(nameof(TestCollections.Default))]
public sealed class ConfigurationControllerTests
{
    private readonly ApplicationFactoryFixture _applicationFactoryFixture;

    public ConfigurationControllerTests(ApplicationFactoryFixture applicationFactoryFixture)
    {
        _applicationFactoryFixture = applicationFactoryFixture;
    }

    [Fact]
    public async Task ValidAuth_ChangePassword_ShouldReturnOK()
    {
        var client = await _applicationFactoryFixture.WithAdminAuthAsync();

        var settings = _applicationFactoryFixture.Services.GetRequiredService<IOptions<PasswordSettings>>().Value;

        var oldSettings = new PasswordSettings
        {
            RequiredLength = settings.RequiredLength,
            RequireDigit = settings.RequireDigit,
            RequireLowercase = settings.RequireLowercase,
            RequireUppercase = settings.RequireUppercase,
            RequireNonAlphanumeric = settings.RequireNonAlphanumeric,
            PreventPasswordReuseCount = settings.PreventPasswordReuseCount,
            MaxPasswordAge = settings.MaxPasswordAge
        };

        try
        {
            var passwordSettings = new PasswordSettings
            {
                RequiredLength = 2,
                RequireDigit = settings.RequireDigit,
                RequireLowercase = settings.RequireLowercase,
                RequireUppercase = settings.RequireUppercase,
                RequireNonAlphanumeric = settings.RequireNonAlphanumeric,
                PreventPasswordReuseCount = settings.PreventPasswordReuseCount,
                MaxPasswordAge = settings.MaxPasswordAge
            };

            var response = await client.PostAsJsonAsync("configuration/passwordsettings", passwordSettings);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var passwordChangeDto = new PasswordChangeDto("nw", "secret");

            response = await client.PatchAsJsonAsync("user/password", passwordChangeDto);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
        finally
        {
            var response = await client.PostAsJsonAsync("configuration/passwordsettings", oldSettings);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}