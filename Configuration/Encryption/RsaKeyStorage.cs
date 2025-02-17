using System.Security.Cryptography;
using Application.Queries.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace Configuration.Encryption;

public sealed class RsaKeyStorage : IRsaKeyStorage
{
    public static RsaKeyStorage Instance { get; } = new();

    public RsaSecurityKey RsaSecurityKey { get; }

    public RSAParameters PrivateKey { get; }

    public string PublicKey { get; }

    private RsaKeyStorage()
    {
        var rsa = RSA.Create(2_048);
        
        PrivateKey = rsa.ExportParameters(true);
        PublicKey = rsa.ExportRSAPublicKeyPem();

        RsaSecurityKey = new RsaSecurityKey(rsa);
    }
}