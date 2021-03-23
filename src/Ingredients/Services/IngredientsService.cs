using System;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Ingredients.Protos;
using Microsoft.Extensions.Logging;
using Pizza.Data;

namespace Ingredients.Services
{
    class IngredientsService : Protos.IngredientsService.IngredientsServiceBase
    {
        private readonly ILogger _logger;
        private readonly IToppingData _toppingData;
        private readonly ICrustData _crustData;

        public IngredientsService(ILogger<IngredientsService> logger, IToppingData toppingData, ICrustData crustData)
        {
            _logger = logger;
            _toppingData = toppingData;
            _crustData = crustData;
        }

        public override async Task<GetToppingsResponse> GetToppings(GetToppingsRequest request, ServerCallContext context)
        {
            try
            {
                var toppings = await _toppingData.GetAsync(context.CancellationToken);

                var availableToppings = toppings.Select(t => new AvailableTopping
                {
                    Quantity = t.StockCount,
                    Topping = new Topping
                    {
                        Id = t.Id,
                        Name = t.Name,
                        Price = t.Price
                    }
                });

                var response = new GetToppingsResponse
                {
                    Toppings = { availableToppings }
                };

                return response;
            } 
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "Operation was cancelled.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error: " + ex.Message);
                throw;
            }
        }

        public override async Task<GetCrustsResponse> GetCrusts(GetCrustsRequest request, ServerCallContext context)
        {
            try
            {
                var crusts = await _crustData.GetAsync(context.CancellationToken);

                var availableCrusts = crusts.Select(c => new AvailableCrust
                {
                    Quantity = c.StockCount,
                    Crust = new Crust
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Price = c.Price
                    }
                });

                var response = new GetCrustsResponse
                {
                    Crusts = { availableCrusts }
                };

                return response;
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "Operation was cancelled.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error: " + ex.Message);
                throw;
            }
        }
    }
}
