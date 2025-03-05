using Application.Commands.Interfaces;
using Domain.News;
using MongoDB.Driver;

namespace Infrastructure.Repositories;

public sealed class MongoArticleRepository : IArticleRepository
{
    private readonly IMongoCollection<Article> _articles;

    public MongoArticleRepository(IMongoClient client)
    {
        var database = client.GetDatabase("News");
        _articles = database.GetCollection<Article>("Articles");
    }

    public async Task AddAsync(Article article)
    {
        await _articles.InsertOneAsync(article);
    }

    public async Task DeleteAsync(string id)
    {
        await _articles.DeleteOneAsync(s => s.Id == id);
    }

    public async Task<IEnumerable<Article>> GetAllAsync()
    {
        return await _articles.Find(_ => true).ToListAsync();
    }

    public async Task<Article?> GetBySymbolAsync(string id)
    {
        return await _articles.Find(s => s.SymbolId == id).FirstOrDefaultAsync();
    }

    public async Task<long> GetNumberOfArticleForSymbol(string id)
    {
        return await _articles.Find(s => s.SymbolId == id).CountDocumentsAsync();
    }

    public async Task UpdateAsync(Article article)
    {
        await _articles.ReplaceOneAsync(s => s.Id == article.Id, article);
    }
}