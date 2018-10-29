
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace FunctionAppInVSErnesto
{
    public static class AzureDevOpsTrigger
    {
        [FunctionName("AzureDevOpsTrigger")]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequest req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request from Azure DevOps.");

            string name = req.Query["name"];

            string requestBody = new StreamReader(req.Body).ReadToEnd();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            return name != null
                ? ProcessNameandTime(name)
                : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
        }

        private static ActionResult ProcessNameandTime(string name)
        {
            var activationTime = System.DateTime.Now;
            if (name.Equals("activator")&& (activationTime.Minute.ToString().EndsWith("2") || activationTime.Minute.ToString().EndsWith("7")) )
            {
                return new OkObjectResult($"Go Deploy");
            }

            return new BadRequestObjectResult("Wrongly activation");
        }
    }
}
