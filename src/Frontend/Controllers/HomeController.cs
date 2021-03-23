using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Frontend.Models;
using Ingredients.Protos;
using System.Threading;

namespace Frontend.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _log;
        private readonly IngredientsService.IngredientsServiceClient _client;

        public HomeController(ILogger<HomeController> log, IngredientsService.IngredientsServiceClient client)
        {
            _log = log;
            _client = client;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var toppingsResponse = await _client.GetToppingsAsync(new GetToppingsRequest(), cancellationToken: ct);
            var crustsResponse = await _client.GetCrustsAsync(new GetCrustsRequest(), cancellationToken: ct);

            var toppings = toppingsResponse.Toppings.Select(t => new ToppingViewModel
            {
                Id = t.Topping.Id,
                Name = t.Topping.Name,
                Price = Convert.ToDecimal(t.Topping.Price)
            }).ToList();

            var crusts = crustsResponse.Crusts.Select(t => new CrustViewModel
            {
                Id = t.Crust.Id,
                Name = t.Crust.Name,
                Price = Convert.ToDecimal(t.Crust.Price)
            }).ToList();

            var viewModel = new HomeViewModel(toppings, crusts);
            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
