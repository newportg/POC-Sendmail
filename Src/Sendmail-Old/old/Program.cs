//using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
//using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
//using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
//using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using Microsoft.OpenApi.Models;
//using System;
//using System.IO;
//using System.Reflection;

//namespace KnightFrank.Hub.Instructions
//{
//    public class Program
//    {
//        public static void Main()
//        {
//            var host = new HostBuilder()
//                .ConfigureFunctionsWorkerDefaults()
//                .ConfigureOpenApi()
//                .ConfigureServices(services =>
//                {
//                    services.AddSingleton<IOpenApiConfigurationOptions>(_ =>
//                    {
//                        var options = new OpenApiConfigurationOptions()
//                        {
//                            Info = new OpenApiInfo()
//                            {
//                                Version = "1.0.0",
//                                Title = "Poc Sendmail",
//                                Description = "Sendmail API",
//                                TermsOfService = new Uri("https://www.knightfrank.com/legals"),
//                                Contact = new OpenApiContact()
//                                {
//                                    Name = "Contact",
//                                    Url = new Uri("https://www.knightfrank.com/contact"),
//                                }
//                            },
//                            Servers = DefaultOpenApiConfigurationOptions.GetHostNames(),
//                            OpenApiVersion = OpenApiVersionType.V2,
//                            IncludeRequestingHostName = true,
//                            ForceHttps = false,
//                            ForceHttp = false
//                        };

//                        return options;
//                    });
//                })
//                .ConfigureAppConfiguration(config => config
//                    .SetBasePath(Directory.GetCurrentDirectory())
//                    .AddJsonFile("local.settings.json", true, true)
//                    .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
//                    .AddEnvironmentVariables())
//                .ConfigureServices((hostContext, services) =>
//                {
//                    services.AddHttpClient();
//                    services.AddLogging();
//                })
//                .Build();

//            host.Run();
//        }
//    }
//}

