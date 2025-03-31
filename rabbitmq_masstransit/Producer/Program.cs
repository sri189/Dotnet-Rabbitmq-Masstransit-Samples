using Common.Models;
using MassTransit;
using MassTransit.Logging;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// MassTransit configuration
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq://localhost");

        cfg.Message<RoutingMessageModel>(c =>
        {
            c.SetEntityName("log_exchange"); // Set a new exchange name
        });

        cfg.Publish<RoutingMessageModel>(p =>
        {
            p.ExchangeType = "direct"; // Ensure correct exchange type
        });

        cfg.Message<TopicExchangeModel>(x => x.SetEntityName("sensor_exchange")); // Topic Exchange
        cfg.Publish<TopicExchangeModel>(x => x.ExchangeType = "topic");
    });
});

var app = builder.Build();

app.MapPost("/point_to_point", async (IPublishEndpoint publishEndpoint) =>
{
    await publishEndpoint.Publish(new PointToPointModel { Message = "Hello from Minimal API!" });
    return Results.Ok("Message Sent!");
});

app.MapPost("/work_queue", async (IPublishEndpoint publishEndpoint) =>
{
    for (int i = 1; i <= 5; i++)
    {
        await publishEndpoint.Publish(new WorkQueueModel { TaskNumber = i });
        Console.WriteLine($"Sent Task {i}");
    }

    return Results.Ok("Tasks Sent!");
});

app.MapPost("/publish_subscribe", async (IPublishEndpoint publishEndpoint) =>
{
    var message = new PublishSubscribeModel { Message = "Hello Subscribers!" };
    await publishEndpoint.Publish(message);
    return Results.Ok("Message Published");
});

string[] severities = { "Info", "Warning", "Error" }; // Random severity levels
var random = new Random();

app.MapPost("/routing/send", async (IPublishEndpoint publishEndpoint) =>
{
    string severity = severities[random.Next(severities.Length)]; // Pick a random severity

    var message = new RoutingMessageModel
    {
        Severity = severity,
        Text = $"Log with {severity} severity"
    };

    await publishEndpoint.Publish(message, context =>
    {
        context.SetRoutingKey(severity); // Set routing key dynamically
    });

    return Results.Ok($"Log Sent with severity {severity}");
});

app.MapPost("/topic_exchange", async (IPublishEndpoint publishEndpoint) =>
{
    string[] topics = { "sensor.temperature.high", "sensor.vibration.critical", "sensor.error.major" };
    var random = new Random();
    string topic = topics[random.Next(topics.Length)]; // Pick a random topic

    var message = new TopicExchangeModel
    {
        EventType = topic,
        Data = $"Event generated: {topic}"
    };

    await publishEndpoint.Publish(message, context =>
    {
        context.SetRoutingKey(topic); // Set dynamic topic key
    });

    return Results.Ok($"Published message with topic: {topic}");
});

app.MapPost("/request_response", async (IRequestClient<RequestMessage> client) =>
{
    string[] products = { "Laptop", "Smart Phone", "PlayStation" };
    var random = new Random();
    string product = products[random.Next(products.Length)]; // Pick a random topic

    var orderRequest = new RequestMessage
    {
        OrderId = Guid.NewGuid(),
        ProductName = product,
    };

    var response = await client.GetResponse<ResponseMessage>(orderRequest);

    return Results.Ok(new { response.Message.OrderId, response.Message.Status });
});

app.MapPost("/priority_queue", async (IPublishEndpoint publishEndpoint) =>
{
    string[] messages = { "Sample Message One", "Sample Message Two", "Sample Message Three" };
    int[] priorities = { 1, 2,3,4, 5,6,7,8,9,10 };
    var random = new Random(); // Move outside loop for better randomness

    var tasks = new List<Task>();

    for (int i = 0; i < 10; i++)
    {
        var message = new PriorityMessageModel
        {
            Message = messages[random.Next(messages.Length)] + ",Request No - "+i.ToString(),
            Priority = priorities[random.Next(priorities.Length)]
        };

        tasks.Add(publishEndpoint.Publish(message, context =>
        {
            context.Headers.Set("priority", message.Priority);
        }));

        //await publishEndpoint.Publish(message, context =>
        //{
        //    context.Headers.Set("priority", message.Priority);
        //});
    }

    await Task.WhenAll(tasks);

    return Results.Ok("Messages sent with priorities.");
});

app.MapPost("/saga", async (IPublishEndpoint publishEndpoint) =>
{
    var orderId = Guid.NewGuid();
    await publishEndpoint.Publish(new OrderSubmitted(orderId, "Laptop", 1));

    return Results.Ok($"Order {orderId} submitted.");
});

app.Run();