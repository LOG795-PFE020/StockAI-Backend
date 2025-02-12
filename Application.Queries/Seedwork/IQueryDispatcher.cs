using Domain.Common.Monads;

namespace Application.Queries.Seedwork;

public interface IQueryDispatcher
{
    Task<Result<TQueryResult>> DispatchAsync<TQuery, TQueryResult>(TQuery query, CancellationToken cancellation = default)
        where TQuery : IQuery;
}