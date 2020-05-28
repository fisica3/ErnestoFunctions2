using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System.Text;
namespace FunctionAppInVSErnesto
{
    public static class LeeBus
    {
        [FunctionName("LeeBus")]
        public static void Run([ServiceBusTrigger("ejemploacp", "Medellin", Connection = "ejemplobus2000")]Message mySbMsg, ILogger log)
        {
            log.LogInformation($"C# ServiceBus topic trigger function processed message: {mySbMsg.MessageId}");
            var content = Encoding.ASCII.GetString(mySbMsg.Body, 0, mySbMsg.Body.Length); 
            log.LogInformation(content);
        }
    }
}
