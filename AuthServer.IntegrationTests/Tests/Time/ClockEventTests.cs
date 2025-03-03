using Application.Commands.TimeMultiplier;
using AuthServer.IntegrationTests.Infrastructure;
using System.Net.Http.Json;
using AuthServer.IntegrationTests.Tests.Rabbitmq.Framework;
using Domain.Time.DomainEvents;
using FluentAssertions;

namespace AuthServer.IntegrationTests.Tests.Time;

[Collection(nameof(TestCollections.Default))]
public sealed class ClockEventTests
{
    private readonly ApplicationFactoryFixture _applicationFactoryFixture;

    public ClockEventTests(ApplicationFactoryFixture applicationFactoryFixture)
    {
        _applicationFactoryFixture = applicationFactoryFixture;
    }

    [Fact]
    public async Task WithHighTimeMultiplier_FirstReceivedDayStartedEvent_ShouldBeTomorrow()
    {
        // one day is 86400 seconds
        const int multiplier = 86400;

        var testId = Guid.NewGuid();

        var client = await _applicationFactoryFixture.WithClientUserAuthAsync(testId);

        var response = await client.PatchAsJsonAsync("time", new ChangeClockTimeMultiplier(multiplier));

        response.EnsureSuccessStatusCode();

        var dayStartedEvent = await MessageSink.ListenFor<DayStarted>("*", new CancellationTokenSource(10_000).Token);

        dayStartedEvent.Should().NotBeNull();

        dayStartedEvent.Length.Should().Be(1);

        dayStartedEvent.Single().NewDay.Date.Should().Be(DateTime.UtcNow.Add(TimeSpan.FromDays(1)).Date);
    }
}