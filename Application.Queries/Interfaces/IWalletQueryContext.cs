using Domain.Portfolio;

namespace Application.Queries.Interfaces;

public interface IWalletQueryContext
{
    IQueryable<Wallet> Wallets { get; }
}