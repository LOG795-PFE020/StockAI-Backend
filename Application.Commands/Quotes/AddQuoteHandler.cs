using Application.Commands.Interfaces;
using Application.Commands.Seedwork;
using Domain.Common.Monads;

namespace Application.Commands.Quotes;

public sealed class AddQuoteHandler : ICommandHandler<AddQuote>
{
    private readonly ISharesRepository _repository;

    private static readonly SemaphoreSlim SemaphoreSlim = new(1);

    public AddQuoteHandler(ISharesRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(AddQuote command, CancellationToken cancellation)
    {
        await SemaphoreSlim.WaitAsync(cancellation);

        try
        {
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
            SemaphoreSlim.Release();
        }
    }
}