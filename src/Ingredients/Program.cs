using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AuthHelp;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Hosting;

namespace Ingredients
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Activity.DefaultIdFormat = ActivityIdFormat.W3C;
            CreateHostBuilder(args).Build().Run();
        }

        // Additional configuration is required to successfully run gRPC on macOS.
        // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();

                    webBuilder.ConfigureKestrel(options =>
                   {
                       options.ConfigureHttpsDefaults(defaults =>
                       {
                           var serverCert = ServerCert.Get();
                           defaults.ServerCertificate = serverCert;
                           defaults.ClientCertificateMode = ClientCertificateMode.RequireCertificate;
                           defaults.ClientCertificateValidation = (cert, _, _) => cert.Issuer == serverCert.Issuer;
                       });
                   });
                });
    }
}
