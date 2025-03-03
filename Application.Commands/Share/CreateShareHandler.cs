using Application.Commands.Interfaces;
using Application.Commands.Seedwork;
using Domain.Common.Monads;

namespace Application.Commands.Share;

public sealed class CreateShareHandler : ICommandHandler<CreateShare>
{
    private readonly ISharesRepository _repository;

    public CreateShareHandler(ISharesRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(CreateShare command, CancellationToken cancellation)
    {
        if (_repository.GetBySymbolAsync(command.Symbol) is { } share)
        {
            return Result.Failure($"Share with symbol '{command.Symbol}' already exists");
        }

        var newShare = new Domain.Stock.Share(command.Symbol);

        await _repository.AddAsync(newShare);

        return Result.Success();
    }
}