using Application.Queries.Seedwork;

namespace Application.Queries.Price;

public record GetStockPrice(string Symbol, DateTime DateTime) : IQuery;