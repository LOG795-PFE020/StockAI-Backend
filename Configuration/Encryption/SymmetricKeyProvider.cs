using Application.Queries.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace Configuration.Encryption;

public sealed class SymmetricKeyProvider : ISymmetricKeyProvider
{
    public SymmetricSecurityKey SigningKey { get; } = new(RandomNumberGenerator.GetBytes(32));
}