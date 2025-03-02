using System.Collections.Immutable;
using AuthServer.IntegrationTests.Tests.Rabbitmq.Messages;

namespace AuthServer.IntegrationTests.Tests.Rabbitmq.Framework;

internal static class MessageSink
{
    private static ImmutableHashSet<ITestMessage> _messages = [];

    public static void AddMessage(ITestMessage testMessage)
    {
        ImmutableInterlocked.Update(ref _messages, set => set.Add(testMessage));
    }

    public static async ValueTask<TMessage> ListenFor<TMessage>(Guid correlationId, CancellationToken token) where TMessage : ITestMessage
    {
        while (token.IsCancellationRequested is false)
        {
            if (TryGetMessage<TMessage>(correlationId) is {} message) return message;

            await Task.Delay(100, token);
        }

        throw new TimeoutException($"No message of type {typeof(TMessage).Name} with correlation id {correlationId} was received.");
    }

    private static TMessage? TryGetMessage<TMessage>(Guid testId) where TMessage : ITestMessage
    {
        return _messages.OfType<TMessage>().SingleOrDefault(message => message.CorrelationId.Equals(testId));
    }
}