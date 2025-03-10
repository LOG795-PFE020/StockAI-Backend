using Application.Queries.Seedwork;

namespace Application.Queries.ShareVolume;

public record GetSharesVolume(string WalletId) : IQuery;