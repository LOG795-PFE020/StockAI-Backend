using Domain.News;

namespace Application.Commands.Interfaces;

public interface IArticleRepository
{
    Task AddAsync(Article article);
    Task DeleteAsync(string id);
    Task<IEnumerable<Article>> GetAllAsync();
    Task<Article?> GetBySymbolAsync(string id);
    Task<long> GetNumberOfArticleForSymbol(string id);
    Task UpdateAsync(Article article);
}