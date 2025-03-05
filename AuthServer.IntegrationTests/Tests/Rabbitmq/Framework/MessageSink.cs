using System.Collections.Immutable;
using Domain.Common.Seedwork.Abstract;

namespace AuthServer.IntegrationTests.Tests.Rabbitmq.Framework;

internal static class MessageSink
{
    private static ImmutableHashSet<Event> _messages = [];

    public static void AddMessage(Event testMessage)
    {
        ImmutableInterlocked.Update(ref _messages, set => set.Add(testMessage));
    }

    public static async ValueTask<TMessage[]> ListenFor<TMessage>(CancellationToken token) where TMessage : Event
    {
        while (token.IsCancellationRequested is false)
        {
            var messages = _messages.OfType<TMessage>().ToArray();

            if (messages.Any())
            {
                return messages;
            }

            await Task.Delay(100, token);
        }

        throw new TimeoutException($"No message of type {typeof(TMessage).Name} was received.");
    }
}