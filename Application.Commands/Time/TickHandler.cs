using Application.Commands.Interfaces;
using Application.Commands.Seedwork;
using Domain.Common.Monads;

namespace Application.Commands.Time;

public sealed class TickHandler : ICommandHandler<ClockTick>
{
    private readonly IInMemoryStore<Domain.Time.Clock> _memoryStore;
    private readonly IMessagePublisher _messagePublisher;

    public TickHandler(IInMemoryStore<Domain.Time.Clock> memoryStore, IMessagePublisher messagePublisher)
    {
        _memoryStore = memoryStore;
        _messagePublisher = messagePublisher;
    }

    public async Task<Result> Handle(ClockTick command, CancellationToken cancellation)
    {
        var clock = _memoryStore.Values.Single();

        clock.Tick();

        foreach (var domainEvent in clock.DomainEvents)
        {
            await _messagePublisher.Publish(domainEvent);
        }

        return Result.Success();
    }
}