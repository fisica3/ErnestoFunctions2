using System;
using Microsoft.Azure.WebJobs;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using System.Text;
using Microsoft.Extensions.Configuration;
using Azure.Identity;
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
       // private readonly IConfiguration _configuration;
        private IConfigurationRefresher _configurationRefresher;

        /* public LeeBus(IConfiguration configuration, IConfigurationRefresherProvider refresherProvider, IFeatureManagerSnapshot featureManagerSnapshot)
         {
             isLocal = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID"));            
             _configuration = configuration;
             _featureManagerSnapshot = featureManagerSnapshot;
             _configurationRefresher = refresherProvider.Refreshers.First();
             connString = Environment.GetEnvironmentVariable("SqlServerConnection");
         } */

        static LeeBus()
        {
            var builder = new ConfigurationBuilder();            
            bool isLocal = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID"));
            connString = Environment.GetEnvironmentVariable("SqlServerConnection");
            if (isLocal)
            {
                var appConfLocal = Environment.GetEnvironmentVariable("KeyConnectionString");
                builder.AddAzureAppConfiguration(appConfLocal);
            }
            else
            {
                builder.AddAzureAppConfiguration(options =>
                {
                    options.Connect(new Uri(Environment.GetEnvironmentVariable("EndpointURL")), new ManagedIdentityCredential())
                    .ConfigureKeyVault(kv =>
                    {
                        kv.SetCredential(new DefaultAzureCredential());
                    });
                    //  ConfigurationRefresher = options.GetRefresher();
                });
            }
            _configuration = builder.Build();
            // catch (CredentialUnavailableException e)
            // {
            //     builder = new ConfigurationBuilder();

            //_configuration = builder.Build();
            //}
        }

        [FunctionName("LeeBus")]
        public void Run([ServiceBusTrigger(
                topicName: "%MiLeeBus.Topic%",
                subscriptionName: "%MiLeeBus.Subscription%",
                Connection = "MiLeeBus.Connection")] ServiceBusReceivedMessage mySbMsg, ILogger log)
        {
            log.LogInformation($"C# ServiceBus topic trigger function processed message: {mySbMsg.MessageId}");
            string keyName = "TestApp:Settings:Message02";
            string message = _configuration[keyName];
            //var content = Encoding.ASCII.GetString(mySbMsg.Body, 0, mySbMsg.Body.Length); 
            var content = mySbMsg.Body.ToString();
            log.LogInformation($"Desde SB: {content}. Desde AppConfig: {message}");
            var fechaEmision = mySbMsg.ScheduledEnqueueTime.DateTime.ToLocalTime();            
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
