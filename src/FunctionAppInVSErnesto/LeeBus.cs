using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace FunctionAppInVSErnesto
{
    public static class LeeBus
    {
        private static IConfiguration Configuration { set; get; }
        static LeeBus()
        {
            var builder = new ConfigurationBuilder();
            builder.AddAzureAppConfiguration(Environment.GetEnvironmentVariable("ejemplobus2000"));
            Configuration = builder.Build();
        }
        [FunctionName("LeeBus")]
        public static void Run([ServiceBusTrigger("ejemploacp", "Medellin", Connection = "ejemplobus2000")]Message mySbMsg, ILogger log)
        {
            log.LogInformation($"C# ServiceBus topic trigger function processed message: {mySbMsg.MessageId}");
            var content = Encoding.ASCII.GetString(mySbMsg.Body, 0, mySbMsg.Body.Length); 
            log.LogInformation(content);
        }
    }
}
