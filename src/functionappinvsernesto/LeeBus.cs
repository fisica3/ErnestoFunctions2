using System;
using Microsoft.Azure.WebJobs;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureManagement;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using System.Linq;

namespace FunctionAppInVSErnesto
{
    public class LeeBus
    {
        private static IConfiguration _configuration { set; get; }
        private static bool isLocal;
        private static string connString;
        private readonly IFeatureManagerSnapshot _featureManagerSnapshot;
        private IConfigurationRefresher _configurationRefresher;

        public LeeBus(IConfiguration configuration, IConfigurationRefresherProvider refresherProvider, IFeatureManagerSnapshot featureManagerSnapshot)
         {
             isLocal = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID"));            
             _configuration = configuration;
             _featureManagerSnapshot = featureManagerSnapshot;
             _configurationRefresher = refresherProvider.Refreshers.First();
             connString = Environment.GetEnvironmentVariable("SqlServerConnection");
         } 
//Notese que si bien aqui hacemos usamos MiLeeBus.Connection, en al Application Settings estamos declarando 
//MiLeeBus.Connection__fullyQualifiedNamespace, esto es debido a que queremos el soporte de Managed Identities desde el trigger
         [FunctionName("LeeBus")]
        public void Run([ServiceBusTrigger(
                topicName: "%MiLeeBus.Topic%",
                subscriptionName: "%MiLeeBus.Subscription%",
                Connection = "MiLeeBus.Connection")] ServiceBusReceivedMessage mySbMsg, ILogger log)
        {
            log.LogInformation($"C# ServiceBus topic trigger function processed message: {mySbMsg.MessageId}");
            string keyName = "TestApp:Settings:Message02";
            string message = _configuration[keyName];
            var content = mySbMsg.Body.ToString();
            log.LogInformation($"Desde SB: {content}. Desde AppConfig: {message}");
            var fechaEmision = mySbMsg.EnqueuedTime.DateTime.ToLocalTime();            
            grabaItemCola(mySbMsg.MessageId, $"{content} {message}", fechaEmision, log);
        }

        public void grabaItemCola(string messageId, string message, DateTime fechaEmision, ILogger log)
        {
            var item = new Model.ItemCola
            {
                Message = message,
                MessageId = messageId,
                FechaEmision = fechaEmision,
                FechaHoraRecepcion = System.DateTime.Now
            };
            try
            {
                using (var db = new AppDbContext(connString))
                {
                   /* db.Database.EnsureDeleted();*/
                    db.Database.EnsureCreated();
                    db.ItemColas.Add(item);
                    db.SaveChanges();
                    /*  if (!db.Books.Any())
                      {
                          WriteTestData(db);
                          Console.WriteLine("Seeded database");
                      }*/
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                    throw;
            }



        }
    }
}
