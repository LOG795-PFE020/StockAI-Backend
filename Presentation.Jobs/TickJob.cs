using Application.Commands.Clock;
using Application.Commands.Seedwork;
using Application.Commands.Time;
using Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Presentation.Jobs;

public sealed class TickJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    private const int TickIntervalMs = 1_000;

    public TickJob(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();

        var commandDispatcher = scope.ServiceProvider.GetRequiredService<ICommandDispatcher>();
        var transactionInfo = scope.ServiceProvider.GetRequiredService<ITransactionInfo>();

        var result = await commandDispatcher.DispatchAsync(new CreateClock(), stoppingToken);

        result.ThrowIfException();

        ServiceReady.Instance.Ready<TickJob>();

        while (stoppingToken.IsCancellationRequested is false)
        {
            await Task.Delay(TickIntervalMs, stoppingToken);

            transactionInfo.CorrelationId = Guid.NewGuid().ToString();

            await commandDispatcher.DispatchAsync(new ClockTick(), stoppingToken);
        }
    }
}