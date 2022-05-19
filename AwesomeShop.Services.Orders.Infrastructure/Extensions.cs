using AwesomeShop.Services.Orders.Infrastructure.CacheStorage;
using AwesomeShop.Services.Orders.Infrastructure.db.mongo;
using AwesomeShop.Services.Orders.Infrastructure.MessageBus;
using AwesomeShop.Services.Orders.Infrastructure.ServiceDiscovery;
using Consul;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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

        public static IServiceCollection AddConsulConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(consulConfig =>
            {
                var address = configuration.GetSection("Consul:Host");
                consulConfig.Address = new Uri(address.Value);
            }));

            services.AddTransient<IServiceDiscoveryService, ConsulService>();

            return services;
        }

        public static IApplicationBuilder UseConsul(this IApplicationBuilder app)
        {
            var consulClient = app.ApplicationServices.GetRequiredService<IConsulClient>();
            var lifeTime = app.ApplicationServices.GetRequiredService<IHostApplicationLifetime>();

            var registration = new AgentServiceRegistration
            {
                ID = $"order-service-{Guid.NewGuid()}",
                Name = "OrderServices",
                Address = "localhost",
                Port = 5003
            };

            // Realizar o deregistrar
            consulClient.Agent.ServiceDeregister(registration.ID).ConfigureAwait(true);
            // Realizar o registro
            consulClient.Agent.ServiceRegister(registration).ConfigureAwait(true);

            Console.WriteLine("Service ORDER registed in Consul");

            // Quando a aplicação parar o sistema vai deregistrar
            lifeTime.ApplicationStopped.Register(() =>
            {
                consulClient.Agent.ServiceDeregister(registration.ID).ConfigureAwait(true);

                Console.WriteLine("Service ORDER deregisted in Consul");
            });

            return app;
        }

        public static IServiceCollection AddRedisCache(this IServiceCollection services)
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.InstanceName = "OrdersCache";
                options.Configuration = "localhost:6379";
            });

            services.AddTransient<ICacheService, RedisService>();

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
