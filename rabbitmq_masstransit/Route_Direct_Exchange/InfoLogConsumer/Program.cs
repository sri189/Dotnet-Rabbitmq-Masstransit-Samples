using Common.Models;
using MassTransit;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumer<InfoLogConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host("rabbitmq://localhost");

                cfg.ReceiveEndpoint("log_queue_info", e =>
                {
                    e.ConfigureConsumeTopology = false;
                    e.Bind("log_exchange", s =>
                    {
                        s.RoutingKey = "Info";
                        s.ExchangeType = "direct";
                    });

                    e.ConfigureConsumer<InfoLogConsumer>(context);
                });
            });
        });
    })
    .Build();

await host.RunAsync();

public class InfoLogConsumer : IConsumer<RoutingMessageModel>
{
    public Task Consume(ConsumeContext<RoutingMessageModel> context)
    {
        Console.WriteLine($"[Info Logger] Received: {context.Message.Text}");
        return Task.CompletedTask;
    }
}
