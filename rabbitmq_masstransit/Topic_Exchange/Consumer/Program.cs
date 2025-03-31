using Common.Models;
using MassTransit;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumer<TemperatureSensorConsumer>();

            x.AddConsumer<CriticalAlertConsumer>();

            x.AddConsumer<GeneralLoggerConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host("rabbitmq://localhost");

                cfg.ReceiveEndpoint("temperature_queue", e =>
                {
                    e.ConfigureConsumeTopology = false;
                    e.Bind("sensor_exchange", s =>
                    {
                        s.RoutingKey = "sensor.temperature.*"; // Matches all temperature events
                        s.ExchangeType = "topic";
                    });

                    e.ConfigureConsumer<TemperatureSensorConsumer>(context);
                });

                cfg.ReceiveEndpoint("critical_alert_queue", e =>
                {
                    e.ConfigureConsumeTopology = false;
                    e.Bind("sensor_exchange", s =>
                    {
                        s.RoutingKey = "sensor.*.critical"; // Matches any critical event
                        s.ExchangeType = "topic";
                    });

                    e.ConfigureConsumer<CriticalAlertConsumer>(context);
                });

                cfg.ReceiveEndpoint("general_log_queue", e =>
                {
                    e.ConfigureConsumeTopology = false;
                    e.Bind("sensor_exchange", s =>
                    {
                        s.RoutingKey = "#"; // Match all messages
                        s.ExchangeType = "topic";
                    });

                    e.ConfigureConsumer<GeneralLoggerConsumer>(context);
                });
            });
        });
    })
    .Build();

await host.RunAsync();

public class TemperatureSensorConsumer : IConsumer<TopicExchangeModel>
{
    public Task Consume(ConsumeContext<TopicExchangeModel> context)
    {
        Console.WriteLine($"[Temperature Service] Received: {context.Message.Data}");
        return Task.CompletedTask;
    }
}

public class CriticalAlertConsumer : IConsumer<TopicExchangeModel>
{
    public Task Consume(ConsumeContext<TopicExchangeModel> context)
    {
        Console.WriteLine($"[Critical Alert Service] Received: {context.Message.Data}");
        return Task.CompletedTask;
    }
}

public class GeneralLoggerConsumer : IConsumer<TopicExchangeModel>
{
    public Task Consume(ConsumeContext<TopicExchangeModel> context)
    {
        Console.WriteLine($"[General Logger] Received: {context.Message.Data}");
        return Task.CompletedTask;
    }
}