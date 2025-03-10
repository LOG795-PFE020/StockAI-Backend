using Domain.Common.Seedwork.Abstract;

namespace Application.Commands.NewUser;

public sealed class UserCreated : Event
{
    public string WalletId { get; init; } = string.Empty;
}