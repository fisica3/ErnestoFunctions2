using System.Collections.Generic;
using System.Net.Http;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace FunctionAppInVSErnesto
{
    public static class PrimerDurable
    {
        [FunctionName("PrimerDurable")]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var outputs = new List<string>();

            // Replace "hello" with the name of your Durable Activity Function.
            outputs.Add(await context.CallActivityAsync<string>("PrimerDurable_Hello", "Tokyo"));
            outputs.Add(await context.CallActivityAsync<string>("PrimerDurable_Hello", "Seattle"));
            outputs.Add(await context.CallActivityAsync<string>("PrimerDurable_Hello", "London"));

            // returns ["Hello Tokyo!", "Hello Seattle!", "Hello London!"]
            return outputs;
        }

        [FunctionName("PrimerDurable_Hello")]
        public static string SayHello([ActivityTrigger] string name, ILogger log)
        {
            var rnd = new System.Random(System.DateTime.Now.Millisecond);
            int a = 0;
            int bucle = 100 * rnd.Next(9);
            for (int i = 0; i < bucle; i++) //1580200
            {
                a = rnd.Next(450);
                var temporal = name + a.ToString();
                String.Concat(temporal.OrderBy(c => c));
            }

            log.LogInformation($"Saying hello to {name}.");
            return $"Hello {name} {bucle}!";
        }

        [FunctionName("PrimerDurable_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")]HttpRequestMessage req,
            [DurableClient]IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("PrimerDurable", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}