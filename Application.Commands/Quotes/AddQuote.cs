using Application.Commands.Seedwork;

namespace Application.Commands.Quotes;

public sealed record AddQuote(string Symbol, DateTime Day, decimal Decimal) : ICommand;