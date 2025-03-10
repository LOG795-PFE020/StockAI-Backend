using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Application.Queries.Interfaces;
using Application.Queries.Seedwork;
using Domain.Common.Monads;
using Domain.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace Application.Queries.Jwt;

public sealed class GetJwtForCredentialsHandler : IQueryHandler<GetJwtForCredentials, string>
{
    private readonly IRsaKeyStorage _rsaKeyStorage;
    private readonly UserManager<UserPrincipal> _principalManager;

    public GetJwtForCredentialsHandler(IRsaKeyStorage rsaKeyStorage, UserManager<UserPrincipal> principalManager)
    {
        _rsaKeyStorage = rsaKeyStorage;
        _principalManager = principalManager;
    }

    public async Task<Result<string>> Handle(GetJwtForCredentials query, CancellationToken cancellation)
    {
        var user = await _principalManager.FindByNameAsync(query.Username);

        if (user is null)
        {
            return Result.Failure<string>("Invalid Username");
        }

        var passwordCheck = await _principalManager.CheckPasswordAsync(user, query.Password);

        if (!passwordCheck)
        {
            await _principalManager.AccessFailedAsync(user);

            return Result.Failure<string>("Invalid Password");
        }

        var role = await _principalManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, query.Username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.Role, role.Single()),
        };

        var creds = new SigningCredentials(_rsaKeyStorage.RsaSecurityKey, SecurityAlgorithms.RsaSha256);

        var token = new JwtSecurityToken(
            issuer: "auth",
            audience: "pfe",
            claims: claims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: creds);

        return Result.Success(new JwtSecurityTokenHandler().WriteToken(token));
    }
}