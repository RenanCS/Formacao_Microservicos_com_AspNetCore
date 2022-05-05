using AwesomeShop.Services.Orders.Core.ValueObjects;
using System;

namespace AwesomeShop.Services.Orders.Core.Events
{
    public class OrderCreated : IDomainEvent
    {
        public OrderCreated(Guid id, decimal totalPrice, PaymentAddress paymentAddress, string fullName, string email)
        {
            Id = id;
            TotalPrice = totalPrice;
            PaymentAddress = paymentAddress;
            FullName = fullName;
            Email = email;
        }

        public Guid Id { get; }
        public decimal TotalPrice { get; }
        public PaymentAddress PaymentAddress { get; }
        public string FullName { get; }
        public string Email { get; }
    }
}
