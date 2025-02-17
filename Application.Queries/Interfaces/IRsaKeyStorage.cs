using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace Application.Queries.Interfaces;

public interface IRsaKeyStorage
{
    RsaSecurityKey RsaSecurityKey { get; }

    RSAParameters PrivateKey { get; }

    string PublicKey { get; }
}