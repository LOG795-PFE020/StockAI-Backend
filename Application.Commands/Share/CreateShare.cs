using Application.Commands.Seedwork;

namespace Application.Commands.Share;

public sealed record CreateShare(string Symbol) : ICommand;