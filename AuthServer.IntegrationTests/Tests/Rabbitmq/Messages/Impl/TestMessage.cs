using Domain.Common.Seedwork.Abstract;

namespace AuthServer.IntegrationTests.Tests.Rabbitmq.Messages.Impl;

public sealed class TestMessage : Event
{
    public string Message { get; set; }
}