﻿using AuthServer.IntegrationTests.Infrastructure;
using Bogus;

namespace AuthServer.IntegrationTests.Tests.News.Services;

internal static class NewsPublisher
{
    public const string Symbol = "AAPL";

    public static async Task<Presentation.Consumers.Messages.News> PublishAsync(ApplicationFactoryFixture applicationFactoryFixture, string content = "")
    {
        DateTime date = DateTime.UtcNow;

        if (string.IsNullOrWhiteSpace(content))
        {
            // create bogus content
            content = new Faker().Lorem.Paragraphs(10);
        }

        var news = new Presentation.Consumers.Messages.News
        {
            Title = new Faker().Company.CompanyName(),
            Symbol = Symbol,
            Content = content,
            PublishedAt = date,
            Opinion = Random.Shared.Next(-1, 2)
        };

        return await applicationFactoryFixture.WithMessagePublished(news);
    }
}