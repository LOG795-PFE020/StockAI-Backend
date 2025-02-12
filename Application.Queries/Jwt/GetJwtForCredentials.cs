using Application.Queries.Seedwork;

namespace Application.Queries.Jwt;

public sealed record GetJwtForCredentials(string Username, string Password) : IQuery;