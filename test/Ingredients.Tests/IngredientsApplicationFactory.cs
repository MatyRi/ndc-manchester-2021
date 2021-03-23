using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ingredients.Protos;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using Pizza.Data;
using TestHelpers;

namespace Ingredients.Tests
{
    public class IngredientsApplicationFactory : WebApplicationFactory<Startup>
    {
        public IngredientsService.IngredientsServiceClient CreateGrcpClient()
        {
            var channel = this.CreateGrpcChannel();
            return new IngredientsService.IngredientsServiceClient(channel);
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IToppingData>();
                services.RemoveAll<ICrustData>();

                var toppingEntities = new List<ToppingEntity>
                {
                    new ToppingEntity("cheese", "Cheese", 0.5m, 50),
                    new ToppingEntity("tomato", "Tomato", 0.75m, 100)
                };

                var toppingDataSub = Substitute.For<IToppingData>();
                toppingDataSub.GetAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(toppingEntities));

                var crustEntites = new List<CrustEntity>
                {
                    new CrustEntity("one", "Large", 10, 5, 50),
                    new CrustEntity("two", "Small", 8, 3, 100)
                };

                var crustsDataSub = Substitute.For<ICrustData>();
                crustsDataSub.GetAsync(Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(crustEntites));

                services.AddSingleton(toppingDataSub);
                services.AddSingleton(crustsDataSub);

            });

            base.ConfigureWebHost(builder);
        }
    }
}
