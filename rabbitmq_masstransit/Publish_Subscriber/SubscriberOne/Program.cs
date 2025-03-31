using Common.Models;
using MassTransit;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumer<NotificationConsumerOne>();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host("rabbitmq://localhost");

                cfg.ReceiveEndpoint("subscriber_one_queue", e =>
                {
                    e.ConfigureConsumer<NotificationConsumerOne>(context);
                });
            });
        });
    })
    .Build();

await host.RunAsync();

// Consumer
public class NotificationConsumerOne : IConsumer<PublishSubscribeModel>
{
    public Task Consume(ConsumeContext<PublishSubscribeModel> context)
    {
        Console.WriteLine($"[Subscriber One] Received: {context.Message.Message}");
        return Task.CompletedTask;
    }
}