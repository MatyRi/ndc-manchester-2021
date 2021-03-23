using System;
using System.Threading.Tasks;
using Pizza.Data;
using Xunit;

namespace Ingredients.Tests
{
    public class CrustsTests : IClassFixture<IngredientsApplicationFactory>
    {

        private IngredientsApplicationFactory _factory;

        public CrustsTests(IngredientsApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetsCrusts()
        {
            var client = _factory.CreateGrcpClient();

            var response = await client.GetCrustsAsync(new Protos.GetCrustsRequest());

            Assert.NotEmpty(response.Crusts);
            Assert.Equal(2, response.Crusts.Count);

            /*Assert.Collection(response.Toppings,
                t => 
                );*/

        }
    }
}
