using Common.Models;
using MassTransit;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumer<OrderConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host("rabbitmq://localhost");
                cfg.ReceiveEndpoint("order_queue", e =>
                {
                    e.ConfigureConsumer<OrderConsumer>(context);
                });
            });
        });
    })
    .Build();

await host.RunAsync();


public class OrderConsumer : IConsumer<RequestMessage>
{
    public async Task Consume(ConsumeContext<RequestMessage> context)
    {
        Console.WriteLine($"Processing Order: {context.Message.OrderId} for {context.Message.ProductName}");

        await context.RespondAsync(new ResponseMessage
        {
            OrderId = context.Message.OrderId,
            Status = "Order Processed"
        });
    }
}