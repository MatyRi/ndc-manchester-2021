using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orders.Protos;

namespace ShopConsole
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly OrdersService.OrdersServiceClient _client;

        public Worker(ILogger<Worker> logger, OrdersService.OrdersServiceClient client)
        {
            _logger = logger;
            _client = client;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Waiting for orders...");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var request = new SubscribeRequest();

                    var call = _client.Subscribe(request);

                    await foreach (var response in call.ResponseStream.ReadAllAsync(stoppingToken))
                    {
                        var crust = response.CrustId;
                        var toppings = string.Join(", ", response.ToppingIds);
                        var time = response.Time.ToDateTimeOffset();

                        _logger.LogInformation("{Time} : {Crust} + {Toppings}", crust, toppings, time);
                    }
                }
                catch (OperationCanceledException ex)
                {
                    if (stoppingToken.IsCancellationRequested)
                    {
                        _logger.LogWarning(ex, "Worker stopping");
                        break;
                    }
                }
            }
        }
    }
}
