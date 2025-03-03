using Domain.Common.Monads;
using Domain.Common.Seedwork.Abstract;
using System.Collections.Immutable;

namespace Application.Commands.Interfaces;

public interface IInMemoryStore<TAggregate> where TAggregate : Aggregate<TAggregate>
{
    public ImmutableList<TAggregate> Values { get; }

    public ImmutableList<string> Keys { get; }

    public Result<TAggregate> Get(string key);

    public List<string> QueryKeys(Func<TAggregate, bool> func);

    public List<TAggregate> QueryValues(Func<TAggregate, bool> func);

    public void AddOrUpdate(string key, TAggregate value);

    public void Remove(string key);
}