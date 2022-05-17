using AwesomeShop.Services.Orders.Core.Entities;
using AwesomeShop.Services.Orders.Core.Repositories;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace AwesomeShop.Services.Orders.Infrastructure.Persistence.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly IMongoCollection<Order> _mongoCollection;
        public OrderRepository(IMongoDatabase mongoDatabase)
        {
            _mongoCollection = mongoDatabase.GetCollection<Order>("orders");
        }

        public async Task AddAsync(Order order)
        {
            await _mongoCollection.InsertOneAsync(order);
        }

        public async Task<Order> GetByIdAsync(Guid id)
        {
            return await _mongoCollection
                .Find(c => c.Id == id)
                .SingleOrDefaultAsync();
        }

        public async Task UpdateAsync(Order order)
        {
            await _mongoCollection.ReplaceOneAsync(c => c.Id == order.Id, order);
        }
    }
}
