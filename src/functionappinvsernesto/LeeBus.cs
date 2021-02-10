using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using System.Text;
using Microsoft.Extensions.Configuration;
using Azure.Identity;
using Microsoft.FeatureManagement;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace FunctionAppInVSErnesto
{
    public class LeeBus
    {
        private static IConfiguration _configuration { set; get; }
        private static bool isLocal;
        private readonly IFeatureManagerSnapshot _featureManagerSnapshot;
       // private readonly IConfiguration _configuration;
        private IConfigurationRefresher _configurationRefresher;

        /*public LeeBus(IConfiguration configuration, IConfigurationRefresherProvider refresherProvider, IFeatureManagerSnapshot featureManagerSnapshot)
        {
            isLocal = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID"));
            _configuration = configuration;
            _featureManagerSnapshot = featureManagerSnapshot;
            _configurationRefresher = refresherProvider.Refreshers.First();

        }*/
        static LeeBus()
        {
            var builder = new ConfigurationBuilder();
            bool isLocal = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID"));

            if (isLocal)
            {
                var appConfLocal = Environment.GetEnvironmentVariable("KeyConnectionString");
                builder.AddAzureAppConfiguration(appConfLocal);
            }
            else
            {
                builder.AddAzureAppConfiguration(options =>
                {
                    options.Connect(new Uri(Environment.GetEnvironmentVariable("EndpointURL")), new ManagedIdentityCredential())
                    .ConfigureKeyVault(kv =>
                    {
                        kv.SetCredential(new DefaultAzureCredential());
                    });
                  //  ConfigurationRefresher = options.GetRefresher();
                });
            }
            _configuration = builder.Build();

            // catch (CredentialUnavailableException e)
            // {
            //     builder = new ConfigurationBuilder();
                 
            //_configuration = builder.Build();
            //}

    } 

        [FunctionName("LeeBus")]
        public void Run([ServiceBusTrigger("ejemploacp", "Medellin", Connection = "ejemplobus2000")]Message mySbMsg, ILogger log)
        {
            log.LogInformation($"C# ServiceBus topic trigger function processed message: {mySbMsg.MessageId}");
            //_configurationRefresher.RefreshAsync();
            string keyName = "TestApp:Settings:Message02";
            string message = _configuration[keyName];

            var content = Encoding.ASCII.GetString(mySbMsg.Body, 0, mySbMsg.Body.Length); 
            log.LogInformation($"Desde SB: {content}. Desde AppConfig: {message}");
        }
    }
}
