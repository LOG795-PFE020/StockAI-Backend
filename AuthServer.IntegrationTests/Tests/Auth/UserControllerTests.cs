using AuthServer.IntegrationTests.Infrastructure;
using System.Net;
using System.Net.Http.Json;
using Application.Commands.NewUser;
using Application.Common.Configurations;
using FluentAssertions;
using Application.Common.Dtos;
using Domain.User;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace AuthServer.IntegrationTests.Tests.Auth;

[Collection(nameof(TestCollections.Default))]
public class UserControllerTests
{
    private readonly ApplicationFactoryFixture _applicationFactoryFixture;

    public UserControllerTests(ApplicationFactoryFixture applicationFactoryFixture)
    {
        _applicationFactoryFixture = applicationFactoryFixture;
    }

    [Fact]
    public async Task ValidAuth_Validate_ShouldReturnOK()
    {
        var client = await _applicationFactoryFixture.WithClientUserAuthAsync();

        var response = await client.GetAsync("user/validate");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ValidAuth_ChangePassword_ShouldReturnOK()
    {
        var defaultAdmin = _applicationFactoryFixture.Services.GetRequiredService<IOptions<DefaultAdmin>>();

        var client = await _applicationFactoryFixture.WithAdminAuthAsync();

        var passwordChangeDto = new PasswordChangeDto("newSecret", "secret");

        var response = await client.PatchAsJsonAsync("user/password", passwordChangeDto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify new login
        var httpResponseMessage = await _applicationFactoryFixture.SigninAsync(new UserCredentials()
        {
            Username = defaultAdmin.Value.Username,
            Password = passwordChangeDto.NewPassword
        });

        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);

        var jwt = await httpResponseMessage.Content.ReadAsStringAsync();

        _applicationFactoryFixture.GetRoleFromJwt(jwt).Should().Be(RoleConstants.AdminRole);
    }

    [Fact]
    public async Task ValidAuth_ChangePasswordShortPassword_ShouldReturnBadRequest()
    {
        var client = await _applicationFactoryFixture.WithAdminAuthAsync();

        var passwordChangeDto = new PasswordChangeDto("nw", "secret");

        var response = await client.PatchAsJsonAsync("user/password", passwordChangeDto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task InValidAuth_ChangePassword_ShouldReturnForbidden()
    {
        var client = _applicationFactoryFixture.CreateDefaultClient();

        var passwordChangeDto = new PasswordChangeDto(string.Empty, string.Empty);

        var response = await client.PatchAsJsonAsync("user/password", JsonConvert.SerializeObject(passwordChangeDto));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ValidAuth_CreateUser_ShouldReturnOK()
    {
        var client = await _applicationFactoryFixture.WithAdminAuthAsync();

        var createUser = new CreateUser("newUser", "secret", RoleConstants.Client);

        var response = await client.PostAsJsonAsync("users", createUser);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseMessage = await _applicationFactoryFixture.SigninAsync(new UserCredentials()
        {
            Username = "newUser",
            Password = "secret"
        });

        responseMessage.StatusCode.Should().Be(HttpStatusCode.OK);

        var jwt = await responseMessage.Content.ReadAsStringAsync();

        _applicationFactoryFixture.GetRoleFromJwt(jwt).Should().Be(RoleConstants.Client);
    }
}