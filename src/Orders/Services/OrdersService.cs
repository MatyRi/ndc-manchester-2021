using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Ingredients.Protos;
using Orders.Protos;
using Orders.PubSub;

namespace Orders.Services
{
    public class OrdersService : Protos.OrdersService.OrdersServiceBase
    {
        private readonly IngredientsService.IngredientsServiceClient _ingredientsClient;
        private readonly IOrderPublisher _orderPublisher;

        public OrdersService(IngredientsService.IngredientsServiceClient client, IOrderPublisher orderpUblisher)
        {
            _ingredientsClient = client;
            _orderPublisher = orderpUblisher;
        }

        public override async Task<PlaceOrderResponse> PlaceOrder(PlaceOrderRequest request, ServerCallContext context)
        {
            var now = DateTimeOffset.UtcNow;

            await _orderPublisher.PublishOrder(request.CrustId, request.ToppingIds, now);

            var decrementCrustsRequets = new DecrementCrustsRequest
            {
                CrustIds = { request.CrustId }
            };

            await _ingredientsClient.DecrementCrustsAsync(decrementCrustsRequets);

            var decrementToppingsRequets = new DecrementToppingsRequest
            {
                ToppingIds = { request.ToppingIds }
            };

            await _ingredientsClient.DecrementToppingsAsync(decrementToppingsRequets);

            return new PlaceOrderResponse();
        }
    }
}
