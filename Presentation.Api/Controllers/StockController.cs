using Application.Queries.Price;
using Application.Queries.Quotes;
using Application.Queries.Seedwork;
using Application.Queries.Stocks;
using Domain.Stock.ValueObjects;
using Domain.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Api.Controllers;

[Authorize(Roles = $"{RoleConstants.Client}, {RoleConstants.AdminRole}")]
[ApiController]
[Route("stocks")]
public sealed class StockController : ControllerBase
{
    private readonly IQueryDispatcher _queryDispatcher;

    public StockController(IQueryDispatcher queryDispatcher)
    {
        _queryDispatcher = queryDispatcher;
    }

    [HttpGet("symbols")]
    public async Task<ActionResult<StockSymbols>> GetStocks()
    {
        var result = await _queryDispatcher.DispatchAsync<GetAllSymbols, List<string>>(new GetAllSymbols());

        return result.IsSuccess() ? Ok(new StockSymbols(result.Content!)) : BadRequest(result.Exception!.Message);
    }


    [HttpGet("quotes/{symbol}")]
    public async Task<ActionResult<StockQuotes>> GetQuotes(string symbol)
    {
        var query = new GetQuotes(symbol);

        var result = await _queryDispatcher.DispatchAsync<GetQuotes, Quote[]>(query);

        return result.IsSuccess() ? Ok(new StockQuotes(result.Content!)) : BadRequest(result.Exception!.Message);
    }

    [HttpGet("{symbol}/{date}")]
    public async Task<ActionResult<StockQuotes>> GetPrice(string symbol, DateTime date)
    {
        var query = new GetStockPrice(symbol, date);

        var result = await _queryDispatcher.DispatchAsync<GetStockPrice, decimal>(query);

        return result.IsSuccess() ? Ok(new StockPrice(result.Content!)) : BadRequest(result.Exception!.Message);
    }

    public record StockSymbols(List<string> Symbols);

    public record StockQuotes(Quote[] Quotes);

    public record StockPrice(decimal Value);
}