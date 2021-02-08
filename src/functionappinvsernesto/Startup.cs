using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Text;
using System;
using System.Reflection;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Azure.Identity;
using System.IO;
using Microsoft.FeatureManagement;
using Microsoft.Extensions.Configuration.EnvironmentVariables;

//https://docs.microsoft.com/en-us/azure/azure-app-configuration/quickstart-feature-flag-azure-functions-csharp

[assembly: FunctionsStartup(typeof(FunctionAppInVSErnesto.Startup))]

namespace FunctionAppInVSErnesto
{
    //private static IConfiguration Configuration { set; get; }
    //private static IConfigurationRefresher ConfigurationRefresher { set; get; }
    //private static bool isLocal;
    public class Startup : FunctionsStartup
    {
        public IConfigurationRefresher ConfigurationRefresher { get; private set; }
        private static bool isLocal;
        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            isLocal = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID"));
            if (isLocal)
            {
                var appConfLocal = Environment.GetEnvironmentVariable("KeyConnectionString");
                builder.ConfigurationBuilder.AddAzureAppConfiguration(appConfLocal);
            }
            else
            {
                string cs = Environment.GetEnvironmentVariable("EndpointURL");
                //builder.ConfigurationBuilder.AddAzureAppConfiguration(cs);

                builder.ConfigurationBuilder.AddAzureAppConfiguration(options =>
                {
                //Rol Requerido para AppConfiguration: App Configuration Data Reader
                //Rol Requerido para KeyVault: Key Vault Secrets User
                options.Connect(new Uri(cs), new ManagedIdentityCredential())
                        .ConfigureKeyVault(kv =>
                        {
                            kv.SetCredential(new DefaultAzureCredential());
                        });
                //_configurationRefresher = options.GetRefresher();
            });
            }
        }

        /*  public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
          {
              builder.ConfigurationBuilder.AddAzureAppConfiguration(options =>
              {
                  options.Connect(Environment.GetEnvironmentVariable("ConnectionString"))
                         .Select("_")
                         .UseFeatureFlags();
              });
          } */

        /*
                public TestAppConfig()
                 {
                     var builder = new ConfigurationBuilder();
                     isLocal = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID"));
                     if (isLocal)
                     {
                         var appConfLocal = Environment.GetEnvironmentVariable("KeyConnectionString");
                         builder.AddAzureAppConfiguration(appConfLocal);                
                     }
                     else
                     {
                         builder.AddAzureAppConfiguration(options =>
                         {
                             //Rol Requerido para AppConfiguration: App Configuration Data Reader
                             //Rol Requerido para KeyVault: Key Vault Secrets User
                             options.Connect(new Uri(Environment.GetEnvironmentVariable("EndpointURL")), new ManagedIdentityCredential())
                                 .ConfigureKeyVault(kv =>
                                 {
                                     kv.SetCredential(new DefaultAzureCredential());
                                 });
                             _configurationRefresher = options.GetRefresher();
                         });
                     }

                    _configuration = builder.Build();
                 }  



         */




        public override void Configure(IFunctionsHostBuilder builder)
        {
            //builder.Services.AddAzureAppConfiguration();
            //builder.Services.AddFeatureManagement();
        }

    }
}