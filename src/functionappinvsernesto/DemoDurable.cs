using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace FunctionAppInVSErnesto
{
    public static class DemoDurable
    {
        [FunctionName("LanzaMensajes")]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var outputs = new List<string>();

            // Replace "hello" with the name of your Durable Activity Function.
            outputs.Add(await context.CallActivityAsync<string>("DemoDurable_Hello", "Tokyo"));
            outputs.Add(await context.CallActivityAsync<string>("DemoDurable_Hello", "Seattle"));
            outputs.Add(await context.CallActivityAsync<string>("DemoDurable_Hello", "London"));

            // returns ["Hello Tokyo!", "Hello Seattle!", "Hello London!"]
            return outputs;
        }

        [FunctionName("DemoDurable_Hello")]
        public static string SayHello([ActivityTrigger] string name, ILogger log)
        {
            var rnd = new System.Random(System.DateTime.Now.Millisecond);
            //int a = 0;
            int bucle = 1000 * rnd.Next(4);
            for (int i = 0; i < bucle; i++) //1580200
            {
                log.LogInformation($"Contador {i}");
            }
            log.LogInformation($"Saying hello to {name}.");
            return $"Hello {name}!";
        }

        [FunctionName("DemoDurable_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("LanzaMensajes", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}