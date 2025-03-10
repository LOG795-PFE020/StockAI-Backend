using Application.Commands.Interfaces;
using Application.Queries.Interfaces;
using Domain.Portfolio;
using Infrastructure.Repositories.Contexes;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class WalletRepository : IWalletRepository, IWalletQueryContext
{
    public IQueryable<Wallet> Wallets { get; }

    private readonly WalletContext _context;

    public WalletRepository(WalletContext context)
    {
        _context = context;
        Wallets = _context.Set<Wallet>().AsNoTracking();
    }

    public async Task<Wallet?> GetAsync(string id)
    {
        return await _context.FindAsync<Wallet>(id);
    }

    public async Task AddAsync(Wallet wallet)
    {
        await _context.AddAsync(wallet);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Wallet wallet)
    {
        _context.Update(wallet);
        await _context.SaveChangesAsync();
    }
}