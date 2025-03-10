using Application.Commands.Seedwork;

namespace Application.Commands.News;

public sealed record IndexArticle(string Title, string SymbolId, string Content, DateTime PublishedAt, int Opinion) : ICommand;