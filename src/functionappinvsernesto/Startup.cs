using System;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Azure.Identity;
using Microsoft.FeatureManagement;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;


[assembly: FunctionsStartup(typeof(FunctionAppInVSErnesto.Startup))]

namespace FunctionAppInVSErnesto
{
    //private static IConfiguration Configuration { set; get; }
    //private static IConfigurationRefresher ConfigurationRefresher { set; get; }
    //private static bool isLocal;
    public class Startup : FunctionsStartup
    {
        private static bool isLocal;
        public IConfigurationRefresher ConfigurationRefresher { get; private set; }

        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            isLocal = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID"));
            if (isLocal)
            {
                var appConfLocal = Environment.GetEnvironmentVariable("KeyConnectionString");
                builder.ConfigurationBuilder.AddAzureAppConfiguration(options =>
                {
                    options.Connect(appConfLocal)
                            .ConfigureRefresh(refreshOptions =>
                             refreshOptions.Register("TestApp:Settings:Message02")
                                 .SetCacheExpiration(TimeSpan.FromSeconds(30))
                                 )
                            .UseFeatureFlags(featureFlagOptions => {
                                featureFlagOptions.CacheExpirationInterval = TimeSpan.FromSeconds(20);
                            });
                });
            }
            else
            {
                string cs = Environment.GetEnvironmentVariable("EndpointURL");
                builder.ConfigurationBuilder.AddAzureAppConfiguration(options =>
                {
                    //Rol Requerido para AppConfiguration: App Configuration Data Reader
                    //Rol Requerido para KeyVault: Key Vault Secrets User
                    options.Connect(new Uri(cs), new ManagedIdentityCredential())
                            .ConfigureKeyVault(kv =>
                            {
                                kv.SetCredential(new DefaultAzureCredential());
                            })
                            .ConfigureRefresh(refreshOptions =>
                             refreshOptions.Register("TestApp:Settings:Message")
                                 .SetCacheExpiration(TimeSpan.FromSeconds(30))
                                 );
                });
            }
        }

        public override void Configure(IFunctionsHostBuilder builder)
        {
            if (ConfigurationRefresher != null)
            {
                builder.Services.AddSingleton(ConfigurationRefresher);
            }
            builder.Services.AddAzureAppConfiguration();
            builder.Services.AddFeatureManagement();
        }
    }
}