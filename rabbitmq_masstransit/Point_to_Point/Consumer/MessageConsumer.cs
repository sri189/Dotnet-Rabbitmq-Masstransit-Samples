using Common.Models;
using MassTransit;

namespace Consumer;

public class MessageConsumer : IConsumer<PointToPointModel>
{
    public Task Consume(ConsumeContext<PointToPointModel> context)
    {
        Console.WriteLine($"Received: {context.Message.Message}");
        return Task.CompletedTask;
    }
}
