using Common.Models;
using MassTransit;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumer<WorkMessageConsumer>();
            x.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host("rabbitmq://localhost");
                cfg.ReceiveEndpoint("work_queue", e =>
                {
                    e.ConfigureConsumer<WorkMessageConsumer>(ctx);
                });
            });
        });

        services.AddHostedService<WorkerService>();
    })
    .Build();

await host.RunAsync();

public class WorkMessageConsumer : IConsumer<WorkQueueModel>
{
    public async Task Consume(ConsumeContext<WorkQueueModel> context)
    {
        Console.WriteLine($"Processing Task {context.Message.TaskNumber}");
        await Task.Delay(1000); // Simulate Work
    }
}

public class WorkerService : BackgroundService
{
    protected override async Task ExecuteAsync(System.Threading.CancellationToken stoppingToken)
    {
        Console.WriteLine("Worker Ready...");
        await Task.Delay(-1, stoppingToken);
    }
}