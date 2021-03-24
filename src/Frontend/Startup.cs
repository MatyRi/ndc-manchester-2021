using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Frontend.Auth;
using Grpc.Core;
using Ingredients.Protos;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orders.Protos;

namespace Frontend
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGrpcClient<IngredientsService.IngredientsServiceClient>((provider, options) =>
            {
                var config = provider.GetRequiredService<IConfiguration>();
                options.Address = config.GetServiceUri("Ingredients", "https");
            });

            services.AddHttpClient<AuthHelper>().ConfigureHttpClient((provider, client) =>
            {
                var config = provider.GetRequiredService<IConfiguration>();
                client.BaseAddress = config.GetServiceUri("Orders", "https") ?? new Uri("https://localhost:5005"); // TODO Test with Configuration
                client.DefaultRequestVersion = new Version(2, 0);
            });

            services.AddGrpcClient<OrdersService.OrdersServiceClient>((provider, options) =>
            {
                var config = provider.GetRequiredService<IConfiguration>();
                options.Address = config.GetServiceUri("Orders", "https") ?? new Uri("https://localhost:5005");
            }).ConfigureChannel((provider, channel) => {

                var authHelper = provider.GetRequiredService<AuthHelper>();
                var credentials = CallCredentials.FromInterceptor(async (context, metadata) =>
                {
                    var token = await authHelper.GetTokenAsync();
                    metadata.Add("Authorization", $"Bearer {token}");
                });

                channel.Credentials = ChannelCredentials.Create(new SslCredentials(), credentials);
            });

            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
