using System;
using System.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace FunctionAppInVSErnesto
{
    public static class ReadStorageQueue
    {
        [FunctionName("ReadStorageQueue")]
        public static void Run([QueueTrigger("democola", Connection = "AzureWebJobsStorage")]string myQueueItem, ILogger log)
        {
            var rnd = new System.Random(System.DateTime.Now.Millisecond);
            int a=0;
            for (int i = 0; i < 1180200; i++)
            {
                a = rnd.Next(450);
                var temporal = myQueueItem + a.ToString();
                String.Concat(temporal.OrderBy(c => c));
            }
            
            log.LogInformation($"**El Trigger de la cola proceso: {myQueueItem} ");
        }
    }
}
