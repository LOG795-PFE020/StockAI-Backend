using Application.Queries.Seedwork;

namespace Application.Queries.Encryption;

public sealed record DecryptCredentials(string EncryptedData) : IQuery;