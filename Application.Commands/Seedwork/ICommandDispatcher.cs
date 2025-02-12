using Domain.Common.Monads;

namespace Application.Commands.Seedwork;

public interface ICommandDispatcher
{
    Task<Result> DispatchAsync<TCommand>(TCommand command, CancellationToken cancellation = default) where TCommand : ICommand;
}