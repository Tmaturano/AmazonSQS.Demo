namespace Messages;

public interface IEvent
{
}

public class OrderCreatedEvent : IEvent
{
    public OrderCreatedEvent(Guid orderId, Guid customerId)
    {
        OrderId = orderId;
        CustomerId = customerId;
        CreatedDate = DateTime.UtcNow;
    }

    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }
    public DateTime CreatedDate { get; init; }
}
