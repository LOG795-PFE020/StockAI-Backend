using Application.Queries.RelevantNews;
using Application.Queries.Seedwork;
using Domain.News;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Presentation.Api.Controllers;

class NewsController : ControllerBase
{
    private readonly IQueryDispatcher _queryDispatcher;
    private readonly ILogger<NewsController> _logger;

    public NewsController(IQueryDispatcher queryDispatcher, ILogger<NewsController> logger)
    {
        _queryDispatcher = queryDispatcher;
        _logger = logger;
    }


    [HttpGet("get")]
    public ActionResult GetNews()
    {

        return Ok(_queryDispatcher.DispatchAsync<RelevantNews, SimpleNews>(new()).Result.GetValueOrThrow());
    }
}

