using Common.Models;
using MassTransit;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumer<WarningLogConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host("rabbitmq://localhost");

                cfg.ReceiveEndpoint("log_queue_warning", e =>
                {
                    e.ConfigureConsumeTopology = false;
                    e.Bind("log_exchange", s =>
                    {
                        s.RoutingKey = "Warning";
                        s.ExchangeType = "direct";
                    });

                    e.ConfigureConsumer<WarningLogConsumer>(context);
                });
            });
        });
    })
    .Build();

await host.RunAsync();

public class WarningLogConsumer : IConsumer<RoutingMessageModel>
{
    public Task Consume(ConsumeContext<RoutingMessageModel> context)
    {
        Console.WriteLine($"[Warning Logger] Received: {context.Message.Text}");
        return Task.CompletedTask;
    }
}
