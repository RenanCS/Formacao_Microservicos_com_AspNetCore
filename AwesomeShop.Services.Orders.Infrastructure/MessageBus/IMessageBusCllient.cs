namespace AwesomeShop.Services.Orders.Infrastructure.MessageBus
{
    public interface IMessageBusCllient
    {
        void Publish(object message, string routingKey, string exchange);
    }
}
