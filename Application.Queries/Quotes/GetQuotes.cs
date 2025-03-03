using Application.Queries.Seedwork;

namespace Application.Queries.Quotes;

public record GetQuotes(string Symbol) : IQuery;