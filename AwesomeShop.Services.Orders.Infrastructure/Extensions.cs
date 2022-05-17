using AwesomeShop.Services.Orders.Infrastructure.db.mongo;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Driver;

namespace AwesomeShop.Services.Orders.Infrastructure
{
    public static class Extensions
    {
        public static IServiceCollection AddMongo(this IServiceCollection services)
        {

            services.AddSingleton(serviceProvider =>
            {
                var configuration = serviceProvider.GetService<IConfiguration>();
                var database = configuration.GetSection("Mongo:Database");
                var connection = configuration.GetSection("Mongo:ConnectionString");

                var options = new MongoDbOptions(database.Value, connection.Value);

                return options;

            });

            services.AddSingleton<IMongoClient>(serviceProvider =>
           {
               var option = serviceProvider.GetService<MongoDbOptions>();
               return new MongoClient(option.ConnectionString);
           });

            services.AddTransient(serviceProvider =>
            {
                BsonDefaults.GuidRepresentation = GuidRepresentation.Standard;

                var options = serviceProvider.GetService<MongoDbOptions>();
                var mongoClient = serviceProvider.GetService<IMongoClient>();

                return mongoClient.GetDatabase(options.Database);

            });

            return services;
        }
    }
}
