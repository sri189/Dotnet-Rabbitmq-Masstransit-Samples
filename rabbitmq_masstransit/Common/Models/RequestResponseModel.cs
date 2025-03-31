namespace Common.Models;

public class RequestMessage
{
    public Guid OrderId { get; set; }
    public string ProductName { get; set; } = default!;
}

public class ResponseMessage
{
    public Guid OrderId { get; set; }
    public string Status { get; set; } = default!;
}