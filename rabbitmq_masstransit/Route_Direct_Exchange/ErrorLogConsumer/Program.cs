using Common.Models;
using MassTransit;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumer<ErrorLogConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host("rabbitmq://localhost");

                cfg.ReceiveEndpoint("log_queue_error", e =>
                {
                    e.ConfigureConsumeTopology = false;
                    e.Bind("log_exchange", s =>
                    {
                        s.RoutingKey = "Error";
                        s.ExchangeType = "direct";
                    });

                    e.ConfigureConsumer<ErrorLogConsumer>(context);
                });
            });
        });
    })
    .Build();

await host.RunAsync();

public class ErrorLogConsumer : IConsumer<RoutingMessageModel>
{
    public Task Consume(ConsumeContext<RoutingMessageModel> context)
    {
        Console.WriteLine($"[Error Logger] Received: {context.Message.Text}");
        return Task.CompletedTask;
    }
}
