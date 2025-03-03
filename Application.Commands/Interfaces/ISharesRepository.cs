namespace Application.Commands.Interfaces;

public interface ISharesRepository
{
    Task<IEnumerable<Domain.Stock.Share>> GetAllAsync();
    Task<Domain.Stock.Share?> GetBySymbolAsync(string id);
    Task AddAsync(Domain.Stock.Share share);
    Task UpdateAsync(Domain.Stock.Share share);
    Task DeleteAsync(string id);
}