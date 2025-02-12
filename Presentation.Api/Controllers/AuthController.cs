using Application.Queries.Seedwork;
using Microsoft.AspNetCore.Mvc;
using Application.Common.Dtos;
using Application.Queries.Encryption;
using Application.Queries.Interfaces;
using Application.Queries.Jwt;
using Domain.Common.Monads;
using Microsoft.Extensions.Logging;

namespace Presentation.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IQueryDispatcher _queryDispatcher;
    private readonly IRsaKeyStorage _keyStorage;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IQueryDispatcher queryDispatcher, IRsaKeyStorage keyStorage, ILogger<AuthController> logger)
    {
        _queryDispatcher = queryDispatcher;
        _keyStorage = keyStorage;
        _logger = logger;
    }

    [HttpGet("publickey")]
    public ActionResult GetPublicKey()
    {
        return Ok(new ServerPublicKey (){ PublicKey = _keyStorage.PublicKey });
    }

    [HttpPost("signin")]
    public async Task<ActionResult> Login([FromBody] EncryptedCredentials encryptedCredentials)
    {
        var decryptCredentials = new DecryptCredentials(encryptedCredentials.EncryptedData);

        var result = await _queryDispatcher.DispatchAsync<DecryptCredentials, UserCredentials>(decryptCredentials)
            .BindAsync(async decryptedCredentials =>
            {
                var createJwt = new GetJwtForCredentials(decryptedCredentials.Username, decryptedCredentials.Password);

                return await _queryDispatcher.DispatchAsync<GetJwtForCredentials, string>(createJwt, CancellationToken.None);
            });

        if (result.IsSuccess()) return Ok(result.Content);

        _logger.LogError(result.Exception!.Message);

        return Forbid();
    }
}