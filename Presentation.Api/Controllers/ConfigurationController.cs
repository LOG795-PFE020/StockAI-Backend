using Application.Common.Configurations;
using Domain.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Api.Controllers;

[Authorize(Roles = RoleConstants.AdminRole)]
[ApiController]
[Route("configuration")]
public sealed class ConfigurationController : ControllerBase
{
    private readonly PasswordSettings _passwordSettings;

    public ConfigurationController(PasswordSettings passwordSettings)
    {
        _passwordSettings = passwordSettings;
    }

    [HttpPost("passwordsettings")]
    public ActionResult PostPasswordSettings([FromBody] PasswordSettings settings)
    {
        _passwordSettings.RequiredLength = settings.RequiredLength;
        _passwordSettings.RequireDigit = settings.RequireDigit;
        _passwordSettings.RequireLowercase = settings.RequireLowercase;
        _passwordSettings.RequireUppercase = settings.RequireUppercase;
        _passwordSettings.RequireNonAlphanumeric = settings.RequireNonAlphanumeric;
        _passwordSettings.PreventPasswordReuseCount = settings.PreventPasswordReuseCount;
        _passwordSettings.MaxPasswordAge = settings.MaxPasswordAge;

        return Ok();
    }

    [HttpGet("passwordsettings")]
    public ActionResult<PasswordSettings> GetPasswordSettings()
    {
        return Ok(_passwordSettings);
    }
}