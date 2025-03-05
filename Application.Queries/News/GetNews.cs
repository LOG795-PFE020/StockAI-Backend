using Application.Queries.Seedwork;

namespace Application.Queries.News;

public sealed record GetNews(string SymbolId) : IQuery;