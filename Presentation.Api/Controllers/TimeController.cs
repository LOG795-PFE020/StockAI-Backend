using Application.Commands.Seedwork;
using Application.Commands.TimeMultiplier;
using Application.Queries.Seedwork;
using Application.Queries.Time;
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
    private readonly IQueryDispatcher _queryDispatcher;

    public TimeController(ICommandDispatcher commandDispatcher, IQueryDispatcher queryDispatcher)
    {
        _commandDispatcher = commandDispatcher;
        _queryDispatcher = queryDispatcher;
    }

    [HttpPatch]
    public async Task<ActionResult> SetTimeMultiplier(ChangeClockTimeMultiplier changeClockTimeMultiplier)
    {
        var result = await _commandDispatcher.DispatchAsync(changeClockTimeMultiplier);

        if (result.IsSuccess()) return Ok();

        return BadRequest(result.Exception!.Message);
    }

    [HttpGet]
    public async Task<ActionResult<CurrentTime>> GetTime()
    {
        var result = await _queryDispatcher.DispatchAsync<GetCurrentTime, DateTime>(new ());

        return result.IsSuccess() ? Ok(new CurrentTime(result.Content)) : BadRequest(result.Exception!.Message);
    }

    public record CurrentTime(DateTime Value);
}