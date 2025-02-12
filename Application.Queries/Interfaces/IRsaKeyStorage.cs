using System.Security.Cryptography;

namespace Application.Queries.Interfaces;

public interface IRsaKeyStorage
{
    RSAParameters PrivateKey { get; }
    string PublicKey { get; }
}