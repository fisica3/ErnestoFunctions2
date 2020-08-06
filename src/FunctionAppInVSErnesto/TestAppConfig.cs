
using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Hosting;

namespace FunctionAppInVSErnesto
// https://docs.microsoft.com/es-mx/azure/azure-app-configuration/quickstart-azure-functions-csharp
// https://docs.microsoft.com/en-us/azure/azure-app-configuration/howto-integrate-azure-managed-service-identity?tabs=core3x
// https://docs.microsoft.com/en-us/azure/azure-app-configuration/enable-dynamic-configuration-azure-functions-csharp
// https://docs.microsoft.com/en-us/azure/key-vault/general/managed-identity
// https://docs.microsoft.com/en-us/azure/azure-app-configuration/use-key-vault-references-dotnet-core?tabs=cmd%2Ccore2x
{
    public static class TestAppConfig
    {
        private static IConfiguration Configuration { set; get; }
        private static IConfigurationRefresher ConfigurationRefresher { set; get; }
        private static bool isLocal;
        static TestAppConfig()
        {
            var builder = new ConfigurationBuilder();
            isLocal = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID"));
            if (isLocal)
            {
                builder.AddAzureAppConfiguration(Environment.GetEnvironmentVariable("KeyConnectionString"));
            }
            else
            {
                builder.AddAzureAppConfiguration(options =>
                {
                    options.Connect(new Uri(Environment.GetEnvironmentVariable("EndpointURL")), new ManagedIdentityCredential())
                //    options.Connect(Environment.GetEnvironmentVariable("Endpoint"))
                        .ConfigureKeyVault(kv =>
                        {
                            kv.SetCredential(new DefaultAzureCredential());
                        })
                        .ConfigureRefresh(refreshOptions =>
                            refreshOptions.Register("TestApp:Settings:Message")
                                .SetCacheExpiration(TimeSpan.FromSeconds(30))
                        );
                    ConfigurationRefresher = options.GetRefresher();
                });
            }

            Configuration = builder.Build();
        }

        [FunctionName("TestAppConfig")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequest req, ILogger log)
        {
            log.LogInformation("El trigger HTTP con C#, proceso un request.");

            if (!isLocal) await ConfigurationRefresher.RefreshAsync();
            string keyName =  "TestApp:Settings:Message";//"claveSQL";//
            string message = Configuration[keyName];
            return message != null
                ? (ActionResult)new OkObjectResult($"El valor recuperado de AppConfig fue '{message}'")
                : new BadRequestObjectResult($"Please create a key-value with the key '{keyName}' in App Configuration, gracias.");
        }
    }
}
