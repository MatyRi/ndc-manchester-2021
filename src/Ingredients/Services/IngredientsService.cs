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

        public IngredientsService(ILogger<IngredientsService> logger, IToppingData toppingData)
        {
            _logger = logger;
            _toppingData = toppingData;
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
                        Price = (double)t.Price
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
    }
}
