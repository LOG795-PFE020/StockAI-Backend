using Domain.Common.Monads;
using Domain.Common.Seedwork.Abstract;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using Application.Commands.Interfaces;

namespace Infrastructure.Repositories;

public sealed class InMemoryStore<TAggregate> : IInMemoryStore<TAggregate> where TAggregate : Aggregate<TAggregate>
{
    private readonly ConcurrentDictionary<string, TAggregate> _dictionary = new();

    public ImmutableList<TAggregate> Values => _dictionary.Values.ToImmutableList();

    public ImmutableList<string> Keys => _dictionary.Keys.ToImmutableList();

    public Result<TAggregate> Get(string key)
    {
        if (_dictionary.TryGetValue(key, out var value))
            return Result.Success(value);

        return Result.TraceFailure<TAggregate>($"Key {key} not found in cache of type {typeof(TAggregate).Name}");
    }

    public List<string> QueryKeys(Func<TAggregate, bool> func)
    {
        var filteredItems = _dictionary
            .Where(kv => func(kv.Value))
            .Select(kv => kv.Key)
            .ToList();

        return filteredItems;
    }

    public List<TAggregate> QueryValues(Func<TAggregate, bool> func)
    {
        var filteredItems = _dictionary
            .Where(kv => func(kv.Value))
            .Select(kv => kv.Value)
            .ToList();

        return filteredItems;
    }

    public void AddOrUpdate(string key, TAggregate value)
    {
        _dictionary.AddOrUpdate(key, value, (_, _) => value);
    }

    public void Remove(string key)
    {
        _dictionary.TryRemove(key, out var value);

        if (value is IDisposable disposable)
            disposable.Dispose();
    }
}