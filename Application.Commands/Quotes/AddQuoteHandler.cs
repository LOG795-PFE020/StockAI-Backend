using Application.Commands.Interfaces;
using Application.Commands.Seedwork;
using Domain.Common.Monads;

namespace Application.Commands.Quotes;

public sealed class AddQuoteHandler : ICommandHandler<AddQuote>
{
    private readonly ISharesRepository _repository;

    private static readonly Mutex Mutex = new();

    public AddQuoteHandler(ISharesRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(AddQuote command, CancellationToken cancellation)
    {
        try
        {
            Mutex.WaitOne();

            Domain.Stock.Share? share = await _repository.GetBySymbolAsync(command.Symbol);

            if (share is null)
            {
                share = new Domain.Stock.Share(command.Symbol);

                await _repository.AddAsync(share);
            }

            share.AddQuote(command.Day, command.Decimal);

            await _repository.UpdateAsync(share);

            return Result.Success();
        }
        finally
        {
            Mutex.ReleaseMutex();
        }
    }
}