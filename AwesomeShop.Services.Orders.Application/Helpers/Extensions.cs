using AwesomeShop.Services.Orders.Application.Dtos.InputModels;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace AwesomeShop.Services.Orders.Application.Helpers
{
    public static class Extensions
    {
        public static IServiceCollection AddHandlers(this IServiceCollection services)
        {
            services.AddMediatR(typeof(AddOrderCommand));
            return services;
        }
    }
}
