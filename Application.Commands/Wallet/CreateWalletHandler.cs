using System.Collections.Immutable;
using Application.Commands.Interfaces;
using Application.Commands.Seedwork;
using Domain.Common.Monads;
using Domain.Portfolio.ValueObjects;

namespace Application.Commands.Wallet;

public sealed class CreateWalletHandler : ICommandHandler<CreateWallet>   
{
    private readonly IWalletRepository _walletRepository;

    public CreateWalletHandler(IWalletRepository walletRepository)
    {
        _walletRepository = walletRepository;
    }

    public async Task<Result> Handle(CreateWallet command, CancellationToken cancellation)
    {
        await _walletRepository.AddAsync(new Domain.Portfolio.Wallet(command.WalletId, new Money(100_000), []));

        return Result.Success();
    }
}