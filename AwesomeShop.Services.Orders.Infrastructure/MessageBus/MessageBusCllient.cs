using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RabbitMQ.Client;
using System.Text;

namespace AwesomeShop.Services.Orders.Infrastructure.MessageBus
{
    public class MessageBusCllient : IMessageBusCllient
    {
        private readonly IConnection _connection;

        public MessageBusCllient(ProducerConnection producerConnection)
        {
            _connection = producerConnection.Connection;
        }

        public void Publish(object message, string routingKey, string exchange)
        {
            var channel = _connection.CreateModel();

            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            var payload = JsonConvert.SerializeObject(message, settings);
            var body = Encoding.UTF8.GetBytes(payload);

            channel.ExchangeDeclare(exchange, "topic", true);

            channel.BasicPublish(exchange, routingKey, null, body);
        }
    }
}
