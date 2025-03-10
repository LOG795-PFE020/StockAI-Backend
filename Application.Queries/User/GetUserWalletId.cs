using Application.Queries.Seedwork;

namespace Application.Queries.User;

public record GetUserWalletId(string Username) : IQuery;