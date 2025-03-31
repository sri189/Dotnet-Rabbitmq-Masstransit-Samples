using Common.Models;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumer<NotificationConsumerTwo>();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host("rabbitmq://localhost");

                cfg.ReceiveEndpoint("subscriber_two_queue", e =>
                {
                    e.ConfigureConsumer<NotificationConsumerTwo>(context);
                });
            });
        });
    })
    .Build();

await host.RunAsync();

// Consumer
public class NotificationConsumerTwo : IConsumer<PublishSubscribeModel>
{
    public Task Consume(ConsumeContext<PublishSubscribeModel> context)
    {
        Console.WriteLine($"[Subscriber Two] Received: {context.Message.Message}");
        return Task.CompletedTask;
    }
}