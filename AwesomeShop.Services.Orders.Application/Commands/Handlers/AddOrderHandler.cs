using AwesomeShop.Services.Orders.Application.Dtos.InputModels;
using AwesomeShop.Services.Orders.Core.Repositories;
using AwesomeShop.Services.Orders.Infrastructure;
using AwesomeShop.Services.Orders.Infrastructure.MessageBus;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AwesomeShop.Services.Orders.Application.Commands.Handlers
{
    public class AddOrderHandler : IRequestHandler<AddOrderCommand, Guid>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMessageBusCllient _messageBusCllient;
        public AddOrderHandler(IOrderRepository orderRepository, IMessageBusCllient messageBusClient)
        {
            _orderRepository = orderRepository;
            _messageBusCllient = messageBusClient;
        }

        public async Task<Guid> Handle(AddOrderCommand request, CancellationToken cancellationToken)
        {
            var order = request.FactoryAddOrderCommandToOrderEntity();

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
