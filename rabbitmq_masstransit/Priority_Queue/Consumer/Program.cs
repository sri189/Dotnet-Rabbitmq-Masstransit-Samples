using Common.Models;
using MassTransit;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumer<PriorityConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host("rabbitmq://localhost");

                cfg.ReceiveEndpoint("priority_queue", e =>
                {
                    e.ConfigureConsumer<PriorityConsumer>(context);

                    // Enable priority queue
                    e.SetQueueArgument("x-max-priority", 10);
                });
            });
        });
    })
    .Build();

await host.RunAsync();

public class PriorityConsumer : IConsumer<PriorityMessageModel>
{
    public Task Consume(ConsumeContext<PriorityMessageModel> context)
    {
        Console.WriteLine($"[Priority {context.Message.Priority}] Received: {context.Message.Message}");
        return Task.CompletedTask;
    }
}