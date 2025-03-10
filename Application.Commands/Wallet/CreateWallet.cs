using Application.Commands.Seedwork;

namespace Application.Commands.Wallet;

public record CreateWallet(string WalletId) : ICommand;