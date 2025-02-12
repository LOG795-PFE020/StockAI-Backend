using System.Security.Claims;
using Application.Commands.Password;
using Application.Commands.Seedwork;
using Application.Common.Dtos;
using Domain.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Api.Controllers;

[Authorize(Roles = $"{RoleConstants.Client}, {RoleConstants.AdminRole}")]
[ApiController]
[Route("user")]
public sealed class UserController : ControllerBase
{
    private readonly ICommandDispatcher _commandDispatcher;

    public UserController(ICommandDispatcher commandDispatcher)
    {
        _commandDispatcher = commandDispatcher;
    }

    [HttpPatch("password")]
    public async Task<ActionResult> ChangePassword([FromBody] PasswordChangeDto passwordChangeDto)
    {
        var changePassword = new ChangePassword(User.Claims.Single(claim => claim.Type.Equals(ClaimTypes.NameIdentifier)).Value, passwordChangeDto.OldPassword, passwordChangeDto.NewPassword);

        var result = await _commandDispatcher.DispatchAsync(changePassword);

        if (result.IsSuccess()) return Ok();

        return BadRequest(result.Exception!.Message);
    }
}