using System;
using System.IO;
using Microsoft.Azure.WebJobs;
using Azure.Identity;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Extensions.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Azure.Storage.Blobs;

namespace FunctionAppInVSErnesto
{    public static class BlobTriggerCSharp
    {
        private static IConfiguration Configuration { set; get; }
        private static IConfigurationRefresher ConfigurationRefresher { set; get; }
        private static bool isLocal;
        static BlobTriggerCSharp()
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
                    options.Connect(new Uri(Environment.GetEnvironmentVariable("EndpointURL")), new ManagedIdentityCredential());
                    ConfigurationRefresher = options.GetRefresher();
                });
            }

            Configuration = builder.Build();
        }

        [FunctionName("BlobTriggerCSharp")]
        public static void Run([BlobTrigger("cookbookfiles2/{name}", Connection = "AzureWebJobsStorage")]Stream myBlob, string name, ILogger log)
        {
            var folderTarget = "destino";
            var connectionTarget = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            string keyName = "CopyPrefix";
            string prefix = Configuration[keyName];
            log.LogInformation($"***Lab*****Función disparada por cambio en blob \n Name:{name} \n Size: {myBlob.Length} Bytes");
            if (name.Contains(".svg"))
            {
                BlobContainerClient containerTarget = new BlobContainerClient(connectionTarget, folderTarget);
                containerTarget.CreateIfNotExists();
                BlobClient blobTarget = containerTarget.GetBlobClient($"{prefix}_{name}");
                blobTarget.Upload(myBlob);
                log.LogInformation($"***Lab***** File {name} copied into {folderTarget}");
            }
        }
    }
}
