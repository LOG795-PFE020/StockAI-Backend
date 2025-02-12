using Application.Common.Dtos;
using Application.Queries.Seedwork;
using Domain.Common.Monads;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Application.Queries.Interfaces;

namespace Application.Queries.Encryption;

public sealed class DecryptCredentialsHandler : IQueryHandler<DecryptCredentials, UserCredentials>
{
    private readonly IRsaKeyStorage _rsaKeyStorage;

    public DecryptCredentialsHandler(IRsaKeyStorage rsaKeyStorage)
    {
        _rsaKeyStorage = rsaKeyStorage;
    }

    public Task<Result<UserCredentials>> Handle(DecryptCredentials query, CancellationToken cancellation)
    {
        byte[] decryptedData;
        using (var rsa = RSA.Create())
        {
            rsa.ImportParameters(_rsaKeyStorage.PrivateKey);
            decryptedData = rsa.Decrypt(Convert.FromBase64String(query.EncryptedData), RSAEncryptionPadding.OaepSHA256);
        }

        var jsonCredentials = Encoding.UTF8.GetString(decryptedData);

        var jsonSettings = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var userCredentials = JsonSerializer.Deserialize<UserCredentials>(jsonCredentials, jsonSettings)!;

        return Task.FromResult(Result.Success(userCredentials));
    }
}