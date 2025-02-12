using Application.Queries.Seedwork;
using Domain.Common.Monads;

namespace Configuration.Dispatchers;

public class QueryDispatcher : IQueryDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<QueryDispatcher> _logger;

    public QueryDispatcher(IServiceProvider serviceProvider, ILogger<QueryDispatcher> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task<Result<TQueryResult>> DispatchAsync<TQuery, TQueryResult>(TQuery query, CancellationToken cancellation)
        where TQuery : IQuery
    {
        try
        {
            var handler = _serviceProvider.CreateScope().ServiceProvider
                .GetRequiredService<IQueryHandler<TQuery, TQueryResult>>();

            if (query.GetType().Name is { } queryName && string.IsNullOrWhiteSpace(queryName) is false)
            {
                _logger.LogTrace($"Dispatching Query '{queryName}'");
            }

            var queryResult = await handler.Handle(query, cancellation);

            if (queryResult.IsFailure())
            {
                _logger.LogError(queryResult.Exception, $"Error while handling query: '{query.GetType().Name}'");
            }

            return queryResult;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error while handling query: '{query.GetType().Name}'");

            return Result.Failure<TQueryResult>(e);
        }
    }
}