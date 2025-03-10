using Application.Commands.Interfaces;
using Application.Commands.Seedwork;
using Domain.Common.Monads;

namespace Application.Commands.TimeMultiplier;

public sealed class ChangeClockTimeMultiplierHandler : ICommandHandler<ChangeClockTimeMultiplier>
{
    private readonly IInMemoryStore<Domain.Time.Clock> _memoryStore;

    public ChangeClockTimeMultiplierHandler(IInMemoryStore<Domain.Time.Clock> memoryStore)
    {
        _memoryStore = memoryStore;
    }

    public Task<Result> Handle(ChangeClockTimeMultiplier command, CancellationToken cancellation)
    {
        var clock = _memoryStore.Values.Single();

        clock.SetMultiplier(command.Multiplier);

        return Task.FromResult(Result.Success());
    }
}