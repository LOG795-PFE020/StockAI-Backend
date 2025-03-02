using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using Application.Common.Dtos;
using AuthServer.IntegrationTests.Infrastructure;
using Domain.User;
using FluentAssertions;
using Newtonsoft.Json;
using Presentation.Jobs;

namespace AuthServer.IntegrationTests.Tests.Auth;

[Collection(nameof(TestCollections.Default))]
public class TestAuthentification
{
    private readonly ApplicationFactoryFixture _applicationFactoryFixture;

    public TestAuthentification(ApplicationFactoryFixture applicationFactoryFixture)
    {
        _applicationFactoryFixture = applicationFactoryFixture;
    }

    [Theory]
    [InlineData("John", "secret", RoleConstants.Client)]
    public async Task AuthenticatedUser_ShouldReceiveOK(string username, string password, string role)
    {
        var unauthorizedClient = _applicationFactoryFixture.CreateDefaultClient();

        await AddDefaultDbRecords.IsReady.Task;

        // Fetch public key from the server
        var publicKeyResponse = await unauthorizedClient.GetFromJsonAsync<ServerPublicKey>("auth/publickey");

        string pemPublicKey = publicKeyResponse.PublicKey;

        using var rsa = RSA.Create();

        rsa.ImportFromPem(pemPublicKey.ToCharArray());  // Import public key

        var userCredentials = new UserCredentials
        {
            Username = username,
            Password = password
        };

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

        // Check the response status code
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        string jwt = await response.Content.ReadAsStringAsync();

        _applicationFactoryFixture.GetRoleFromJwt(jwt).Should().Be(role);
    }
}