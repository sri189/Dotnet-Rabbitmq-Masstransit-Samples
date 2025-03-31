using MassTransit;

public class Worker : BackgroundService
{
    private readonly IBusControl _bus;

    public Worker(IBusControl bus)
    {
        _bus = bus;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _bus.StartAsync(stoppingToken);
        Console.WriteLine("Waiting for messages...");
        await Task.Delay(-1, stoppingToken);
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        await _bus.StopAsync(stoppingToken);
    }
}