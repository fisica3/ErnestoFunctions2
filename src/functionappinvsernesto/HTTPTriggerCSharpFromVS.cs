
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FunctionAppInVSErnesto
{
    public static class HTTPTriggerCSharpFromVS
    {
        [FunctionName("HTTPTriggerCSharpFromVS")]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequest req, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = new StreamReader(req.Body).ReadToEnd();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            if (name != null)
            {
                log.LogInformation($"OJO!! El valor capturado es: {name}");
                return (ActionResult)new OkObjectResult($"Hello, {name}");
            }
            else return new BadRequestObjectResult("Por favor pase el parametro name en el querystring o en el body del POST");
        }
    }
}
