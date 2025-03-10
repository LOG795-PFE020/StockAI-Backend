using Application.Commands.NewUser;
using Application.Commands.Seedwork;
using Application.Commands.Wallet;
using MassTransit;

namespace Presentation.Consumers;

public sealed class UserCreatedConsumer : IConsumer<UserCreated>
{
    private readonly ICommandDispatcher _commandDispatcher;

    public UserCreatedConsumer(ICommandDispatcher commandDispatcher)
    {
        _commandDispatcher = commandDispatcher;
    }

    public async Task Consume(ConsumeContext<UserCreated> context)
    {
        UserCreated userCreated = context.Message;

        var result = await _commandDispatcher.DispatchAsync(new CreateWallet(userCreated.WalletId));

        result.ThrowIfException();
    }
}