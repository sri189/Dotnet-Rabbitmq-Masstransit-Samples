namespace Common.Models;
public record OrderSubmitted(Guid OrderId, string ProductName, int Quantity);
public record InventoryReserved(Guid OrderId);
public record PaymentProcessed(Guid OrderId);
public record OrderCompleted(Guid OrderId);
public record OrderFailed(Guid OrderId, string Reason);
