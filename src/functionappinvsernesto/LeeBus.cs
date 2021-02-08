using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using System.Text;
using Microsoft.Extensions.Configuration;
using Azure.Identity;
using Microsoft.Extensions.Configuration.EnvironmentVariables;

namespace FunctionAppInVSErnesto
{
    public static class LeeBus
    {
        private static IConfiguration Configuration { set; get; }
        
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
            Configuration = builder.Build();
            
           /* catch (CredentialUnavailableException e)
            {
                builder = new ConfigurationBuilder();
                
                Configuration = builder.Build();
            }*/
            
        }
        //Usando Referencias KeyVault https://docs.microsoft.com/en-us/azure/app-service/app-service-key-vault-references 
        [FunctionName("LeeBus")]
        public static void Run([ServiceBusTrigger("ejemploacp", "Medellin", Connection = "ejemplobus2000")]Message mySbMsg, ILogger log)
        {
            log.LogInformation($"C# ServiceBus topic trigger function processed message: {mySbMsg.MessageId}");

            string keyName = "TestApp:Settings:Message";
            string message = Configuration[keyName];

            var content = Encoding.ASCII.GetString(mySbMsg.Body, 0, mySbMsg.Body.Length); 
            log.LogInformation($"Desde SB: {content}. Desde AppConfig: {message}");
        }
    }
}
