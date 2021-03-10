using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Linq;
using Microsoft.FeatureManagement;
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
        private readonly IFeatureManagerSnapshot _featureManagerSnapshot;
        private readonly IConfiguration _configuration;
        private IConfigurationRefresher _configurationRefresher;

        public TestAppConfig(IConfiguration configuration, IConfigurationRefresherProvider refresherProvider, IFeatureManagerSnapshot featureManagerSnapshot)
        {
            isLocal = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID"));
            _configuration = configuration;
            _featureManagerSnapshot = featureManagerSnapshot;
            _configurationRefresher = refresherProvider.Refreshers.First();

        }

    [FunctionName("TestAppConfig")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequest req, ILogger log)
        {
            log.LogInformation("El trigger HTTP con C#, proceso un request.");
            string keyVaultEntry = "proxymusk";
            string messageKeyVault = "keyvault es local";
            await _configurationRefresher.TryRefreshAsync();
            if (!isLocal)
            {
                await _configurationRefresher.RefreshAsync();
                messageKeyVault = _configuration[keyVaultEntry];
            }
            
            bool flag = await _featureManagerSnapshot.IsEnabledAsync("ActivacionMensaje");
            string keyName =  "TestApp:Settings:Message02";
            string message = _configuration[keyName];
            
            return message != null
                ? (ActionResult)new OkObjectResult($"El valor recuperado desde AppConfig fue '{message}', y el valor desde KeyVault fue '{messageKeyVault}', el FeatureFlag devolvio '{flag}' desde Azure.")
                : new BadRequestObjectResult($"Please create a key-value with the key '{keyName}' in App Configuration, gracias.");
        }
    }
}
