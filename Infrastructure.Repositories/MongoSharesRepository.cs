using Application.Commands.Interfaces;
using Domain.Stock;
using MongoDB.Driver;

namespace Infrastructure.Repositories;

public sealed class MongoSharesRepository : ISharesRepository
{
    private readonly IMongoCollection<Share> _shares;

    public MongoSharesRepository(IMongoClient client)
    {
        var database = client.GetDatabase("Stocks");
        _shares = database.GetCollection<Share>("Shares");
    }

    public async Task AddAsync(Share share)
    {
        await _shares.InsertOneAsync(share);
    }

    public async Task DeleteAsync(string id)
    {
        await _shares.DeleteOneAsync(s => s.Id == id);
    }

    public async Task<IEnumerable<Share>> GetAllAsync()
    {
        return await _shares.Find(_ => true).ToListAsync();
    }

    public async Task<Share?> GetBySymbolAsync(string id)
    {
        return await _shares.Find(s => s.Id == id).FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(Share share)
    {
        await _shares.ReplaceOneAsync(s => s.Id == share.Id, share);
    }
}