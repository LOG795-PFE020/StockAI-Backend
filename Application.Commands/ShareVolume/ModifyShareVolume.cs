using Application.Commands.Seedwork;
using Domain.Portfolio.ValueObjects;

namespace Application.Commands.ShareVolume;

public record ModifyShareVolume(string Symbol, Money Price, int Volume, string WalletId) : ICommand;