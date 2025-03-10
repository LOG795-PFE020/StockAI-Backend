namespace Application.Commands.Interfaces;

public interface IWalletRepository
{
    Task<Domain.Portfolio.Wallet?> GetAsync(string id);
    Task AddAsync(Domain.Portfolio.Wallet wallet);
    Task UpdateAsync(Domain.Portfolio.Wallet wallet);
}