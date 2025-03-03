using Application.Common.Interfaces;

namespace Infrastructure.RabbitMQ.Services;

public sealed class TransactionInfo : ITransactionInfo
{
    public string? CorrelationId { get; set; }
}