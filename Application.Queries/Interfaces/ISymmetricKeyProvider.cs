using Microsoft.IdentityModel.Tokens;

namespace Application.Queries.Interfaces;

public interface ISymmetricKeyProvider
{
    public SymmetricSecurityKey SigningKey { get; }
}