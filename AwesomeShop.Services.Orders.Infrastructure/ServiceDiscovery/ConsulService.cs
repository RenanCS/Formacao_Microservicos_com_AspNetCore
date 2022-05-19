using Consul;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AwesomeShop.Services.Orders.Infrastructure.ServiceDiscovery
{
    public class ConsulService : IServiceDiscoveryService
    {
        private readonly IConsulClient _consulClient;

        public ConsulService(IConsulClient consulClient)
        {
            _consulClient = consulClient;
        }

        public async Task<Uri> GetServiceUri(string serviceName, string requestUrl)
        {
            var allRegistredService = await _consulClient.Agent.Services();

            var registredServices = allRegistredService.Response?
                .Where(service => service.Value.Service == serviceName)
                .Select(service => service.Value)
                .ToList();

            var service = registredServices.FirstOrDefault();

            if (service != null)
            {
                Console.WriteLine($"ConsulService => {service.Address}");

                // localhost: Port: 5002, requestUrl api/custromers/123456789
                var uri = $"http://{service.Address}:{service.Port}/{requestUrl}";

                return new Uri(uri);
            }

            Console.WriteLine($"********** ConsulService => Não encontrado **********");

            return new Uri(null);
        }
    }
}
