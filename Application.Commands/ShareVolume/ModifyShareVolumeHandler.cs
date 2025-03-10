using Application.Commands.Interfaces;
using Application.Commands.Seedwork;
using Domain.Common.Monads;

namespace Application.Commands.ShareVolume;

public sealed class ModifyShareVolumeHandler : ICommandHandler<ModifyShareVolume>
{
    private readonly IWalletRepository _walletRepository;

    public ModifyShareVolumeHandler(IWalletRepository walletRepository)
    {
        _walletRepository = walletRepository;
    }

    public async Task<Result> Handle(ModifyShareVolume command, CancellationToken cancellation)
    {
         Domain.Portfolio.Wallet? wallet = await _walletRepository.GetAsync(command.WalletId);

        if (wallet is null) return Result.Failure("Wallet not found");

        Result result = int.IsPositive(command.Volume) ? 
            wallet.TryBuyStock(command.Symbol, command.Price, command.Volume) : 
            wallet.TrySellStock(command.Symbol, command.Price, Math.Abs(command.Volume));

        if (result.IsFailure()) return result;

        await _walletRepository.UpdateAsync(wallet);

        return Result.Success();
    }
}