using Application.Commands.Seedwork;
using Application.Commands.TimeMultiplier;
using Application.Common.Interfaces;
using Domain.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Api.Controllers;

[Authorize(Roles = $"{RoleConstants.Client}, {RoleConstants.AdminRole}")]
[ApiController]
[Route("time")]
public sealed class TimeController : ControllerBase
{
    private readonly ICommandDispatcher _commandDispatcher;

    public TimeController(ICommandDispatcher commandDispatcher)
    {
        _commandDispatcher = commandDispatcher;
    }

    [HttpPatch]
    public async Task<ActionResult> SetTimeMultiplier(ChangeClockTimeMultiplier changeClockTimeMultiplier)
    {
        var result = await _commandDispatcher.DispatchAsync(changeClockTimeMultiplier);

        if (result.IsSuccess()) return Ok();

        return BadRequest(result.Exception!.Message);
    }
}