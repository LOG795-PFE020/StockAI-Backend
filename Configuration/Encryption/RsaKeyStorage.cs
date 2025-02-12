using System.Security.Cryptography;
using Application.Queries.Interfaces;

namespace Configuration.Encryption;

public sealed class RsaKeyStorage : IRsaKeyStorage
{
    public RSAParameters PrivateKey { get; }

    public string PublicKey { get; }

    public RsaKeyStorage()
    {
        using var rsa = RSA.Create(2_048);
        
        PrivateKey = rsa.ExportParameters(true);
        PublicKey = rsa.ExportRSAPublicKeyPem();
    }
}