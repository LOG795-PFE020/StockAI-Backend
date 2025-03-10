using Application.Commands.Interfaces;
using Application.Common.Interfaces;
using MassTransit;

namespace Infrastructure.RabbitMQ.Services;

public sealed class MessagePublisher : IMessagePublisher
{
    private readonly string _connectionString;
    private readonly ISendEndpointProvider _endpointProvider;
    private readonly ITransactionInfo _transactionInfo;

    public MessagePublisher(string connectionString, ISendEndpointProvider endpointProvider, ITransactionInfo transactionInfo)
    {
        _connectionString = connectionString;
        _endpointProvider = endpointProvider;
        _transactionInfo = transactionInfo;
    }

    public async Task Publish<TMessage>(TMessage message) where TMessage : Domain.Common.Seedwork.Abstract.Event
    {
        Type messageConcreteType = message.GetType();

        var exchangeName = Registration.MassTransit.ExchangeNamesForMessageTypes[messageConcreteType];

        var endpoint = await _endpointProvider.GetSendEndpoint(new Uri($"{_connectionString}/{exchangeName}"));

        await endpoint.Send(message, messageConcreteType, context => context.CorrelationId = _transactionInfo.CorrelationId ?? Guid.NewGuid());
    }
}