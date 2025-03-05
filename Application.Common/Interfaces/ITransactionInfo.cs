namespace Application.Common.Interfaces;

public interface ITransactionInfo
{
    public Guid? CorrelationId { get; set; }
}