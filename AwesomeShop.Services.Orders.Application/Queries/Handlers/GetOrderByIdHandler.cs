﻿using AwesomeShop.Services.Orders.Application.Dtos.InputModels;
using AwesomeShop.Services.Orders.Application.Dtos.ViewModels;
using AwesomeShop.Services.Orders.Core.Repositories;
using AwesomeShop.Services.Orders.Infrastructure.CacheStorage;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace AwesomeShop.Services.Orders.Application.Queries.Handlers
{
    public class GetOrderByIdHandler : IRequestHandler<GetOderByIdQuery, OrderViewModel>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICacheService _cacheService;
        public GetOrderByIdHandler(IOrderRepository orderRepository, ICacheService cacheService)
        {
            _orderRepository = orderRepository;
            _cacheService = cacheService;
        }
        public async Task<OrderViewModel> Handle(GetOderByIdQuery request, CancellationToken cancellationToken)
        {
            var cacheKey = request.Id.ToString();

            var orderViewModel = await _cacheService.GetAsync<OrderViewModel>(cacheKey);

            if (orderViewModel == null)
            {
                var order = await _orderRepository.GetByIdAsync(request.Id);

                if (order == null)
                {
                    return null;
                }

                orderViewModel = OrderViewModel.FactoryOrderToOrderViewModel(order);

                await _cacheService.SetAsync(cacheKey, orderViewModel);
            }

            return orderViewModel;
        }
    }
}
