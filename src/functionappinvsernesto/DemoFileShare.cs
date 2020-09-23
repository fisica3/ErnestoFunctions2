using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;
using Azure.Storage.Files.Shares;

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
            string fileName = req.Query["filename"];

            if (shareName != null && fileName!=null)
            {
                log.LogInformation($"OJO!! El nombre del Share es: {shareName}, y el nombre de file es {fileName}");
                var connectionString = "DefaultEndpointsProtocol=https;AccountName=sgdemofunctions049db1;AccountKey=v2VH1EexsBO7LK2O5FFcziWJInXFQ79olIw8dp3cw64k71c1V7z5UiXLrUZULOXXtw1Q7mt5J6SonxeM1zOCkA==;EndpointSuffix=core.windows.net";
                DownloadAndCopy(connectionString,shareName, fileName, log);
                return (ActionResult)new OkObjectResult($"Hello, {shareName}");
            }
            else return new BadRequestObjectResult("Por favor pase los parametros en el querystring");
        }

        public static void DownloadAndCopy(string connectionString, string shareName, string fileName, ILogger log)
        {
            string dirName = "prueba-dir";

            // Get a reference to the file
            ShareClient share = new ShareClient(connectionString, shareName);
            ShareDirectoryClient directory = share.GetDirectoryClient(dirName);
            ShareFileClient file = directory.GetFileClient(fileName);

            var uriFile = file.Uri;
            //Parametrizar
            var containerName = "destino";           

            BlobContainerClient container = new BlobContainerClient(connectionString, containerName);
            BlobClient blob = container.GetBlobClient(fileName);

            // Upload local file
            
            blob.Upload(file.Download().Value.Content);
            log.LogInformation($"Archivo {fileName} copiado en container {containerName}");
        }
               
    }
}
