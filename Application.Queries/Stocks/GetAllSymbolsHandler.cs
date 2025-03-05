using Application.Queries.Seedwork;
using Domain.Common.Monads;
using Domain.Stock;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Application.Queries.Stocks;

public sealed class GetAllSymbolsHandler : IQueryHandler<GetAllSymbols, List<string>>
{
    private readonly IMongoCollection<Share> _shares;

    public GetAllSymbolsHandler(IMongoClient client)
    {
        var database = client.GetDatabase("Stocks");
        _shares = database.GetCollection<Share>("Share");
    }

    public async Task<Result<List<string>>> Handle(GetAllSymbols query, CancellationToken cancellation)
    {
        var symbols = await _shares.DistinctAsync(share => share.Symbol, share => true, cancellationToken: cancellation);

        return Result.Success(await symbols.ToListAsync(cancellationToken: cancellation));
    }
}