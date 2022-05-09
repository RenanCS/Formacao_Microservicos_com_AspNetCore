using AwesomeShop.Services.Orders.Application.Dtos.ViewModels;
using MediatR;
using System;

namespace AwesomeShop.Services.Orders.Application.Dtos.InputModels
{
    public class GetOderByIdQuery : IRequest<OrderViewModel>
    {
        public GetOderByIdQuery(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; private set; }

    }
}
