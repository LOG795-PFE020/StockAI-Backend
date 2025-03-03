using System.Net;
using System.Net.Http.Json;
using Application.Commands.TimeMultiplier;
using AuthServer.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace AuthServer.IntegrationTests.Tests.Time;

[Collection(nameof(TestCollections.Default))]
public sealed class TimeControllerTest
{
    private readonly ApplicationFactoryFixture _applicationFactoryFixture;

    public TimeControllerTest(ApplicationFactoryFixture applicationFactoryFixture)
    {
        _applicationFactoryFixture = applicationFactoryFixture;
    }

    [Fact]
    public async Task WithValidAuth_SetTimeMultiplier_ShouldReturnOk()
    {
        const int multiplier = 2;
        
        var testId = Guid.NewGuid();
        
        var client = await _applicationFactoryFixture.WithAdminAuthAsync(testId);

        var response = await client.PatchAsJsonAsync("time", new ChangeClockTimeMultiplier(multiplier));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}