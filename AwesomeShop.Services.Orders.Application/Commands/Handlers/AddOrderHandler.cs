using AwesomeShop.Services.Orders.Application.Dtos.InputModels;
using AwesomeShop.Services.Orders.Application.Dtos.IntegrationDtos;
using AwesomeShop.Services.Orders.Core.Repositories;
using AwesomeShop.Services.Orders.Infrastructure;
using AwesomeShop.Services.Orders.Infrastructure.MessageBus;
using AwesomeShop.Services.Orders.Infrastructure.ServiceDiscovery;
using MediatR;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AwesomeShop.Services.Orders.Application.Commands.Handlers
{
    public class AddOrderHandler : IRequestHandler<AddOrderCommand, Guid>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMessageBusCllient _messageBusCllient;
        private readonly IServiceDiscoveryService _serviceDiscoveryService;
        public AddOrderHandler(IOrderRepository orderRepository, IMessageBusCllient messageBusClient, IServiceDiscoveryService serviceDiscoverySerice)
        {
            _orderRepository = orderRepository;
            _messageBusCllient = messageBusClient;
            _serviceDiscoveryService = serviceDiscoverySerice;
        }

        public async Task<Guid> Handle(AddOrderCommand request, CancellationToken cancellationToken)
        {
            var order = request.FactoryAddOrderCommandToOrderEntity();

            var customerUrl = await _serviceDiscoveryService.GetServiceUri("CustumerServices", $"api/customers/{order.Customer.Id}");
            if (customerUrl != null)
            {
                var httpClient = new HttpClient();
                var result = await httpClient.GetAsync(customerUrl);
                var stringResult = await result.Content.ReadAsStringAsync();
                var customerDto = JsonConvert.DeserializeObject<GetCustomerByIdDto>(stringResult);
                Console.WriteLine($"******AddOrderHandle  => CustomerServices => {stringResult} ***********");
            }


            await _orderRepository.AddAsync(order);

            foreach (var @event in order.Events)
            {
                // OrderCreated = order-created
                var routingKey = @event.GetType().Name.ToDashCase();

                _messageBusCllient.Publish(@event, routingKey, "order-service");
            }

            return order.Id;
        }
    }
}
