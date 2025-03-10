using AuthServer.IntegrationTests.Infrastructure;
using System.Net.Http.Json;
using Application.Queries.ShareVolume;
using AuthServer.IntegrationTests.Tests.Stocks.Services;
using Bogus;
using FluentAssertions;

namespace AuthServer.IntegrationTests.Tests.Portfolio;

[Collection(nameof(TestCollections.Default))]
public sealed class PortfolioControllerTests
{
    private readonly ApplicationFactoryFixture _applicationFactoryFixture;

    public PortfolioControllerTests(ApplicationFactoryFixture applicationFactoryFixture)
    {
        _applicationFactoryFixture = applicationFactoryFixture;
    }

    [Fact]
    public async Task WithValidConfiguration_GetShareVolumes_ShouldReturnValidShareVolumesViewModel()
    {
        var fake = new Faker();
        var symbol = fake.Company.CompanyName();

        await QuotePublisher.PublishQuote(_applicationFactoryFixture, symbol);

        var client = await _applicationFactoryFixture.WithClientUserAuthAsync();

        var response = await client.PatchAsync($"portfolio/buy/{symbol}/{10}", null);

        response.EnsureSuccessStatusCode();

        var shareVolumes = await client.GetFromJsonAsync<ShareVolumesViewModel>("portfolio");

        shareVolumes.Should().NotBeNull();

        shareVolumes.ShareVolumes.Should().NotBeEmpty();

        shareVolumes.ShareVolumes.Should().ContainSingle(x => x.Symbol == symbol);

        shareVolumes.ShareVolumes.First(x => x.Symbol == symbol).Volume.Should().Be(10);
    }
}