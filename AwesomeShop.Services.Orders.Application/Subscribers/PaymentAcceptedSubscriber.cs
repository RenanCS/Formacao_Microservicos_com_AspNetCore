using AwesomeShop.Services.Orders.Core.Repositories;
using AwesomeShop.Services.Orders.Core.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AwesomeShop.Services.Orders.Application.Subscribers
{
    public class PaymentAcceptedSubscriber : BackgroundService
    {

        private readonly IServiceProvider _serviceProvider;
        private readonly IConnection _connection;
        private readonly IModel _channel;

        // Fila que vai consumir
        private const string QUEUE = "order-service/payment-accepted";
        private const string EXCHENGE = "order-service";
        // Para realizar a conexão da QUEUE com a fila dos pagamentos (BINDING)
        private const string ROUTINGKEY = "payment-accepted";
        // Canal para ser escutado, a fim de replicar a informação
        private const string EXCHENGEREF = "payment-service";
        // Conexão para comunicar com Rabbit
        private const string CONNECTION = "order-service-payment-accepted-subscriber";


        public PaymentAcceptedSubscriber(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            //var connectionFactory = new ConnectionFactory()
            //{
            //    Uri = new Uri($"amqp://localhost:5672"),
            //    ConsumerDispatchConcurrency = 1,
            //};

            var connectionFactory = new ConnectionFactory
            {
                HostName = "localhost",
                Port = 5673
            };

            // Criar conexão 
            _connection = connectionFactory.CreateConnection(CONNECTION);
            // Criar o canal para o subscriber
            _channel = _connection.CreateModel();
            // Declara a exchenge como topico
            _channel.ExchangeDeclare(EXCHENGE, "topic", true);
            // Declara a fila, caso a mesma não exista
            _channel.QueueDeclare(QUEUE, false, false, false, null);
            /* 
             * Toda vez quem uma msg estiver puublicada na EXCHENGEREF,
             * tendo esta chave de roteamento ROUTINGKEY
             * a msg será criada uma cópia para QUEUE 
             */
            _channel.QueueBind(QUEUE, EXCHENGEREF, ROUTINGKEY);

        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += async (sender, eventArgs) =>
            {
                var byteArray = eventArgs.Body.ToArray();
                var contentString = Encoding.UTF8.GetString(byteArray);
                var message = JsonConvert.DeserializeObject<PaymentAccepted>(contentString);

                var result = await UpdateOrder(message);

                if (result)
                {
                    _channel.BasicAck(eventArgs.DeliveryTag, false);
                }
            };

            _channel.BasicConsume(QUEUE, false, consumer);

            return Task.CompletedTask;
        }

        private async Task<bool> UpdateOrder(PaymentAccepted paymentAccepted)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var orderRepository = scope.ServiceProvider.GetService<IOrderRepository>();
                var order = await orderRepository.GetByIdAsync(paymentAccepted.Id);

                order.SetAsCompleted();

                await orderRepository.UpdateAsync(order);

                return true;
            }
        }


    }
}
