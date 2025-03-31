using Common.Models;
using MassTransit;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddMassTransit(x =>
        {
            x.AddSagaStateMachine<OrderStateMachine, OrderState>()
             .InMemoryRepository();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host("rabbitmq://localhost");
                cfg.ConfigureEndpoints(context);
            });
        });
    })
    .Build();

await host.RunAsync();

public class OrderState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }
}

public class OrderStateMachine : MassTransitStateMachine<OrderState>
{
    public State InventoryPending { get; private set; }
    public State PaymentPending { get; private set; }
    public State Completed { get; private set; }
    public State Failed { get; private set; }

    public Event<OrderSubmitted> OrderSubmitted { get; private set; }
    public Event<InventoryReserved> InventoryReserved { get; private set; }
    public Event<PaymentProcessed> PaymentProcessed { get; private set; }
    public Event<OrderFailed> OrderFailed { get; private set; }

    public OrderStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Event(() => OrderSubmitted, x => x.CorrelateById(m => m.Message.OrderId));
        Event(() => InventoryReserved, x => x.CorrelateById(m => m.Message.OrderId));
        Event(() => PaymentProcessed, x => x.CorrelateById(m => m.Message.OrderId));
        Event(() => OrderFailed, x => x.CorrelateById(m => m.Message.OrderId));

        Initially(
            When(OrderSubmitted)
                .Then(context => Console.WriteLine($"[Saga] Order received: {context.Message.OrderId}"))
                .TransitionTo(InventoryPending)
                .Publish(context => new InventoryReserved(context.Message.OrderId))
        );

        During(InventoryPending,
            When(InventoryReserved)
                .Then(context => Console.WriteLine($"[Saga] Inventory reserved for {context.Message.OrderId}"))
                .TransitionTo(PaymentPending)
                .Publish(context => new PaymentProcessed(context.Message.OrderId))
        );

        During(PaymentPending,
            When(PaymentProcessed)
                .Then(context => Console.WriteLine($"[Saga] Payment processed for {context.Message.OrderId}"))
                .TransitionTo(Completed)
                .Publish(context => new OrderCompleted(context.Message.OrderId)),

            When(OrderFailed)
                .Then(context => Console.WriteLine($"[Saga] Order failed: {context.Message.Reason}"))
                .TransitionTo(Failed)
        );
    }
}
