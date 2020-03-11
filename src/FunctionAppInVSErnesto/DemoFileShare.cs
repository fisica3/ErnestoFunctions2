using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using Newtonsoft.Json;

namespace FunctionAppInVSErnesto
{
    public static class DemoFileShare
    {
        [FunctionName("DemoFileShare")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string shareName = req.Query["sharename"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            shareName = shareName ?? data?.shareName;

            if (shareName != null)
            {
                log.LogInformation($"OJO!! El nombre del Share capturado es: {shareName}");
                var connectionString = "DefaultEndpointsProtocol=https;AccountName=sgdemofunctions049db1;AccountKey=v2VH1EexsBO7LK2O5FFcziWJInXFQ79olIw8dp3cw64k71c1V7z5UiXLrUZULOXXtw1Q7mt5J6SonxeM1zOCkA==;EndpointSuffix=core.windows.net";
                DownloadAndCopy(connectionString,shareName,"", log);
                return (ActionResult)new OkObjectResult($"Hello, {shareName}");
            }
            else return new BadRequestObjectResult("Por favor pase el parametro name en el querystring o en el body del POST");
        }

        /// <summary>
        /// Download a file.
        /// </summary>
        /// <param name="connectionString">
        /// A connection string to your Azure Storage account.
        /// </param>
        /// <param name="shareName">
        /// The name of the share to download from.
        /// </param>
        /// <param name="localFilePath">
        /// Path to download the local file.
        /// </param>
        public static void DownloadAndCopy(string connectionString, string shareName, string localFilePath, ILogger log)
        {
            #region Snippet:Azure_Storage_Files_Shares_Samples_Sample01a_HelloWorld_Download
            //@@ string connectionString = "DefaultEndpointsProtocol=https;AccountName=sgdemofunctions049db1;AccountKey=v2VH1EexsBO7LK2O5FFcziWJInXFQ79olIw8dp3cw64k71c1V7z5UiXLrUZULOXXtw1Q7mt5J6SonxeM1zOCkA==;EndpointSuffix=core.windows.net";

            // Name of the share, directory, and file we'll download from
            //@@ string shareName = "sample-share";
            string dirName = "prueba-dir";
            string fileName = "descarga.png";

            // Path to the save the downloaded file
            //@@ string localFilePath = @"<path_to_local_file>";

            // Get a reference to the file
            ShareClient share = new ShareClient(connectionString, shareName);
            ShareDirectoryClient directory = share.GetDirectoryClient(dirName);
            ShareFileClient file = directory.GetFileClient(fileName);

            var uriFile = file.Uri;
            var containerName = "destino";
            log.LogInformation($"Atenci�n: El nombre del Share capturado es: {shareName}");

            BlobContainerClient container = new BlobContainerClient(connectionString, containerName);
          //  container.Create();

            // Get a reference to a blob named "sample-file" in a container named "sample-container"
            BlobClient blob = container.GetBlobClient("nuevonombre.png");

            // Upload local file
            
            blob.Upload(file.Download().Value.Content);

            /*
            // Download the file
            ShareFileDownloadInfo download = file.Download();
            using (FileStream stream = File.OpenWrite(localFilePath))
            {
                download.Content.CopyTo(stream);
            } */
            #endregion Snippet:Azure_Storage_Files_Shares_Samples_Sample01a_HelloWorld_Download
        }
               
    }
}
