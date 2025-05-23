﻿using Application.Queries.Seedwork;
using Domain.Common.Monads;
using Domain.Stock;
using MongoDB.Driver;

namespace Application.Queries.Price;

public sealed class GetStockPriceHandler : IQueryHandler<GetStockPrice, decimal>
{
    private readonly IMongoCollection<Share> _shares;

    public GetStockPriceHandler(IMongoClient client)
    {
        var database = client.GetDatabase("Stocks");
        _shares = database.GetCollection<Share>("Shares");
    }

    public async Task<Result<decimal>> Handle(GetStockPrice query, CancellationToken cancellation)
    {
        Share? share = await _shares.Find(s => s.Id == query.Symbol).FirstOrDefaultAsync(cancellationToken: cancellation);

        if (share is null)
        {
            return Result.Failure<decimal>($"Share with symbol '{query.Symbol}' not found");
        }

        return share.GetPrice(query.DateTime);
    }
}