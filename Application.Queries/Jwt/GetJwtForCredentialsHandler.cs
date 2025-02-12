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
    private readonly ISymmetricKeyProvider _symmetricKeyProvider;
    private readonly UserManager<UserPrincipal> _principalManager;

    public GetJwtForCredentialsHandler(ISymmetricKeyProvider symmetricKeyProvider, UserManager<UserPrincipal> principalManager)
    {
        _symmetricKeyProvider = symmetricKeyProvider;
        _principalManager = principalManager;
    }

    public async Task<Result<string>> Handle(GetJwtForCredentials command, CancellationToken cancellation)
    {
        var user = await _principalManager.FindByNameAsync(command.Username);

        if (user is null)
        {
            return Result.Failure<string>("Invalid Username");
        }

        var passwordCheck = await _principalManager.CheckPasswordAsync(user, command.Password);

        if (!passwordCheck)
        {
            await _principalManager.AccessFailedAsync(user);

            return Result.Failure<string>("Invalid Password");
        }

        var role = await _principalManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, command.Username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.Role, role.Single())
        };

        var creds = new SigningCredentials(_symmetricKeyProvider.SigningKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            //skipping these for the lab
            issuer: null,
            audience: null,
            claims: claims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: creds);

        return Result.Success(new JwtSecurityTokenHandler().WriteToken(token));
    }
}