using AwesomeShop.Services.Orders.Infrastructure.db.mongo;
using AwesomeShop.Services.Orders.Infrastructure.MessageBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Driver;
using RabbitMQ.Client;
using System;
using System.Text;

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

        public static IServiceCollection AddMessageBus(this IServiceCollection services)
        {
            var connectionFactory = new ConnectionFactory
            {
                HostName = "localhost",
                Port = 5673
            };

            var connection = connectionFactory.CreateConnection("order-service-producer");

            services.AddSingleton(new ProducerConnection(connection));
            services.AddSingleton<IMessageBusCllient, MessageBusCllient>();

            return services;
        }

        public static string ToDashCase(this string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            if (text.Length < 2)
            {
                return text;
            }

            var sb = new StringBuilder();
            sb.Append(char.ToLowerInvariant(text[0]));

            for (int i = 1; i < text.Length; i++)
            {
                char c = text[i];

                if (char.IsUpper(c))
                {
                    sb.Append('-');
                    sb.Append(char.ToLowerInvariant(c));
                }
                else
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }
    }
}
