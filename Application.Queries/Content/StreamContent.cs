using Application.Queries.Seedwork;

namespace Application.Queries.Content;

public record StreamContent(string ArticleId) : IQuery;