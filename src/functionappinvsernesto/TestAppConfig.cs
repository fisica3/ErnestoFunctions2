
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


namespace FunctionAppInVSErnesto
// https://docs.microsoft.com/es-mx/azure/azure-app-configuration/quickstart-azure-functions-csharp
// https://docs.microsoft.com/en-us/azure/azure-app-configuration/howto-integrate-azure-managed-service-identity?tabs=core3x
// https://docs.microsoft.com/en-us/azure/azure-app-configuration/enable-dynamic-configuration-azure-functions-csharp
// https://docs.microsoft.com/en-us/azure/key-vault/general/managed-identity
// https://docs.microsoft.com/en-us/azure/azure-app-configuration/use-key-vault-references-dotnet-core?tabs=cmd%2Ccore2x
{
    public  class TestAppConfig
    {
        //private static IConfiguration Configuration { set; get; }
        //private static IConfigurationRefresher ConfigurationRefresher { set; get; }
        private static bool isLocal;

        private readonly IConfiguration _configuration;
        private IConfigurationRefresher _configurationRefresher;

        /*public Function1(IConfiguration configuration)
        {
            _configuration = configuration;
        } */

        //public TestAppConfig(IConfiguration configuration, IConfigurationRefresher configurationRefresher)
        //{
        //    _configuration = configuration;
        //    _configurationRefresher = configurationRefresher;// refresherProvider.Refreshers.First();
        //}
        public TestAppConfig(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public void xxxxxTestAppConfig()
         {
             var builder = new ConfigurationBuilder();
             isLocal = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID"));
             if (isLocal)
             {
                 var appConfLocal = Environment.GetEnvironmentVariable("KeyConnectionString");
                 builder.AddAzureAppConfiguration(appConfLocal);                
             }
             else
             {
                 builder.AddAzureAppConfiguration(options =>
                 {
                     //Rol Requerido para AppConfiguration: App Configuration Data Reader
                     //Rol Requerido para KeyVault: Key Vault Secrets User
                     options.Connect(new Uri(Environment.GetEnvironmentVariable("EndpointURL")), new ManagedIdentityCredential())
                         .ConfigureKeyVault(kv =>
                         {
                             kv.SetCredential(new DefaultAzureCredential());
                         })
                         .ConfigureRefresh(refreshOptions =>
                             refreshOptions.Register("TestApp:Settings:Message")
                                 .SetCacheExpiration(TimeSpan.FromSeconds(30))
                         );
                     _configurationRefresher = options.GetRefresher();
                 });
             }

           // _configuration = builder.Build();
         } 

        [FunctionName("TestAppConfig")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequest req, ILogger log)
        {
            log.LogInformation("El trigger HTTP con C#, proceso un request.");
            string keyVaultEntry = "proxymusk";
            string messageKeyVault = "keyvault es local";
            if (!isLocal)
            {
                await _configurationRefresher.RefreshAsync();
                messageKeyVault = _configuration[keyVaultEntry];
            }
            string keyName =  "TestApp:Settings:Message02";
            string message = _configuration[keyName];
            
            return message != null
                ? (ActionResult)new OkObjectResult($"El valor recuperado desde AppConfig fue '{message}', y el valor desde KeyVault fue '{messageKeyVault}' el proceso salio OK en el Video")
                : new BadRequestObjectResult($"Please create a key-value with the key '{keyName}' in App Configuration, gracias.");
        }
    }
}
