using Application.Queries.News;
using Application.Queries.Seedwork;
using Domain.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Api.Controllers;

[Authorize(Roles = $"{RoleConstants.Client}, {RoleConstants.AdminRole}")]
[ApiController]
[Route("news")]
public sealed class NewsController : ControllerBase
{
    private readonly IQueryDispatcher _queryDispatcher;

    public NewsController(IQueryDispatcher queryDispatcher)
    {
        _queryDispatcher = queryDispatcher;
    }

    [HttpGet("{symbol}")]
    public async Task<ActionResult<NewsViewModel>> GetNews(string symbol)
    {
        var result = await _queryDispatcher.DispatchAsync<GetNews, NewsViewModel>(new GetNews(symbol));

        return result.IsSuccess() ? Ok(result.Content) : BadRequest(result.Exception!.Message);
    }


    [HttpGet("articles/{id}")]
    public async Task<ActionResult> GetContent(string id)
    {
        var query = new Application.Queries.Content.StreamContent(id);

        var result = await _queryDispatcher.DispatchAsync<Application.Queries.Content.StreamContent, Stream>(query);

        if (result.IsFailure())
        {
            return BadRequest(result.Exception!.Message);
        }

        return File(result.Content, "application/octet-stream");
    }
}
