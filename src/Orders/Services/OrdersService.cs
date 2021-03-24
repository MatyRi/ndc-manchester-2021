using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Ingredients.Protos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Orders.Protos;
using Orders.PubSub;

namespace Orders.Services
{
    public class OrdersService : Protos.OrdersService.OrdersServiceBase
    {
        private readonly ILogger _logger;
        private readonly IngredientsService.IngredientsServiceClient _ingredientsClient;
        private readonly IOrderPublisher _orderPublisher;
        private readonly IOrderMessages _orderMessages;

        public OrdersService(ILogger<OrdersService> logger, IngredientsService.IngredientsServiceClient client, IOrderPublisher orderpUblisher, IOrderMessages orderMessages)
        {
            _logger = logger;
            _ingredientsClient = client;
            _orderPublisher = orderpUblisher;
            _orderMessages = orderMessages;
        }

        [Authorize(JwtBearerDefaults.AuthenticationScheme)]
        public override async Task<PlaceOrderResponse> PlaceOrder(PlaceOrderRequest request, ServerCallContext context)
        {
            var httpContext = context.GetHttpContext();
            var user = httpContext.User;

            _logger.LogInformation("PlaceOrder request from {User}", user.FindFirst(ClaimTypes.Name));

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

        public override async Task Subscribe(SubscribeRequest request, IServerStreamWriter<SubscribeResponse> responseStream, ServerCallContext context)
        {
            while(context.CancellationToken.IsCancellationRequested)
            {
                try
                {
                    var orderMessage = await _orderMessages.ReadAsync(context.CancellationToken);
                    var response = new SubscribeResponse
                    {
                        CrustId = orderMessage.CrustId,
                        ToppingIds = { orderMessage.ToppingIds },
                        Time = orderMessage.Time.ToTimestamp()
                    };

                    await responseStream.WriteAsync(response);

                }
                catch (OperationCanceledException ex)
                {
                    _logger.LogWarning(ex, "Subscriber disconnected");
                    break;
                }
            }
        }
    }
}
