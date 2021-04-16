using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;
using OpenTelemetry.Trace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorkerServiceHelper
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
            .UseWindowsService()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();

                    var openTelemetry = Sdk.CreateTracerProviderBuilder().
               AddSource("Worker Service 1")
               .SetSampler(new AlwaysOnSampler())
               .AddZipkinExporter(b =>
               {
                   var zipkinHostName = Environment.GetEnvironmentVariable("ZIPKIN_HOSTNAME") ?? "localhost";
                   b.Endpoint = new Uri($"http://{zipkinHostName}:9411/api/v2/spans");
               }
               )
               .Build();
                    services.AddSingleton(openTelemetry);
                });


        }
    }
}
