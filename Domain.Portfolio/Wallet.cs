using System.Collections.Immutable;
using Domain.Common.Monads;
using Domain.Common.Seedwork.Abstract;
using Domain.Portfolio.ValueObjects;

namespace Domain.Portfolio;

public sealed class Wallet : Aggregate<Wallet>
{
    public Money Balance { get; private set; }

    public List<ShareVolume> Shares { get; private set; } = [];

    private Wallet(string id) : base(id) { }

    public Wallet(string id, Money balance, List<ShareVolume> shares) : base(id)
    {
        Balance = balance;
        Shares = shares;
    }
    
    public Result TryBuyStock(string symbol, Money price, int volume)
    {
        var total = price * volume;

        if (Balance.CannotAfford(total)) return Result.Failure("Insufficient funds");

        int indexOfShare = Shares.FindIndex(share => share.Symbol.Equals(symbol));

        if (indexOfShare == -1)
        {
            Shares.Add(new ShareVolume(symbol, volume));
        }
        else
        {
            var share = Shares[indexOfShare];
            Shares.RemoveAt(indexOfShare);
            Shares.Add(share + volume);
        }

        Balance -= total;

        return Result.Success();
    }

    public Result TrySellStock(string symbol, Money price, int volume)
    {
        var total = price * volume;

        int indexOfShare = Shares.FindIndex(share => share.Symbol.Equals(symbol));

        if (indexOfShare == -1) return Result.Failure("No shares found");

        var share = Shares[indexOfShare];

        if (share < volume) return Result.Failure("Insufficient shares");

        if (share.Volume == volume)
        {
            Shares.RemoveAt(indexOfShare);
        }
        else
        {
            Shares.RemoveAt(indexOfShare);
            Shares.Add(share - volume);
        }

        Balance += total;

        return Result.Success();
    }

    public void Deposit(Money amount)
    {
        Balance += amount;
    }
}