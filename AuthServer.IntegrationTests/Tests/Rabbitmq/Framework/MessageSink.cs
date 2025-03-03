using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Domain.Common.Seedwork.Abstract;

namespace AuthServer.IntegrationTests.Tests.Rabbitmq.Framework;

internal static class MessageSink
{
    private static ImmutableHashSet<Event> _messages = [];

    public static void AddMessage(Event testMessage)
    {
        ImmutableInterlocked.Update(ref _messages, set => set.Add(testMessage));
    }

    public static async ValueTask<TMessage[]> ListenFor<TMessage>(string regexCorrelationId, CancellationToken token) where TMessage : Event
    {
        while (token.IsCancellationRequested is false)
        {
            if (TryGetMessages<TMessage>(regexCorrelationId, out var messages)) return messages;

            await Task.Delay(100, token);
        }

        throw new TimeoutException($"No message of type {typeof(TMessage).Name} with regex correlation id {regexCorrelationId} was received.");
    }

    private static bool TryGetMessages<TMessage>(string regexCorrelationId, [NotNullWhen(true)] out TMessage[]? messages) where TMessage : Event
    {
        string regexPattern = "^" + Regex.Escape(regexCorrelationId).Replace("\\*", ".*") + "$";

        messages = _messages.OfType<TMessage>().Where(message => Regex.IsMatch(message.CorrelationId ?? string.Empty, regexPattern)).ToArray();

        return messages.Any();
    }
}