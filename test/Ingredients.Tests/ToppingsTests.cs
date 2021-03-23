using System;
using System.Threading.Tasks;
using Pizza.Data;
using Xunit;

namespace Ingredients.Tests
{
    public class ToppingsTests : IClassFixture<IngredientsApplicationFactory>
    {

        private IngredientsApplicationFactory _factory;

        public ToppingsTests(IngredientsApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetsToppings()
        {
            var client = _factory.CreateGrcpClient();

            var response = await client.GetToppingsAsync(new Protos.GetToppingsRequest());

            Assert.NotEmpty(response.Toppings);
            Assert.Equal(2, response.Toppings.Count);

            /*Assert.Collection(response.Toppings,
                t => 
                );*/

        }
    }
}
