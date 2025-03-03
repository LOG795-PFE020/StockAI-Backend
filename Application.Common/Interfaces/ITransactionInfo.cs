namespace Application.Common.Interfaces;

public interface ITransactionInfo
{
    public string? CorrelationId { get; set; }
}