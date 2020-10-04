using System;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Extensions.Storage;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;

namespace FunctionAppInVSErnesto
{    public static class BlobTriggerCSharp
    {
        [FunctionName("BlobTriggerCSharp")]
        public static void Run([BlobTrigger("cookbookfiles2/{name}", Connection = "AzureWebJobsStorage")]Stream myBlob, string name, ILogger log)
        {
            var folderTarget = "destino";
            var connectionTarget = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            log.LogInformation($"***Lab*****C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");
            if (name.Contains(".svg"))
            {
                BlobContainerClient containerTarget = new BlobContainerClient(connectionTarget, folderTarget);
                BlobClient blobTarget = containerTarget.GetBlobClient($"copied{name}");
                blobTarget.Upload(myBlob);
                log.LogInformation($"***Lab***** File {name} copied into {folderTarget}");
            }
        }
    }
}
