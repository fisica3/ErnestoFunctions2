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

[assembly: FunctionsStartup(typeof(FunctionAppInVSErnesto.Startup))]

namespace FunctionAppInVSErnesto
{
    //private static IConfiguration Configuration { set; get; }
    //private static IConfigurationRefresher ConfigurationRefresher { set; get; }
    //private static bool isLocal;
    public class Startup : FunctionsStartup
    {
        public IConfigurationRefresher ConfigurationRefresher { get; private set; }

        public override void Configure(IFunctionsHostBuilder hostBuilder)
        {
            if (ConfigurationRefresher!=null)
            {
                hostBuilder.Services.AddSingleton(ConfigurationRefresher);
            }
        }
        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder configurationBuilder)
        {
            var hostBuilderContext = configurationBuilder.GetContext();
            var isDevelopment = ("Development" == hostBuilderContext.EnvironmentName);

            if (isDevelopment)
            {
                configurationBuilder.ConfigurationBuilder
                    .AddJsonFile(Path.Combine(hostBuilderContext.ApplicationRootPath, $"appsettings.{hostBuilderContext.EnvironmentName}.json"), optional: true, reloadOnChange: false)
                    .AddUserSecrets<Startup>(optional: true, reloadOnChange: false);
            }

            var configuration = configurationBuilder.ConfigurationBuilder.Build();
            var applicationConfigurationEndpoint = configuration["APPLICATIONCONFIGURATION_ENDPOINT"];

            if (!string.IsNullOrEmpty(applicationConfigurationEndpoint))
            {
                configurationBuilder.ConfigurationBuilder.AddAzureAppConfiguration(appConfigOptions => {
                    var azureCredential = new DefaultAzureCredential(includeInteractiveCredentials: false);

                    appConfigOptions
                        .Connect(new Uri(applicationConfigurationEndpoint), azureCredential)
                        .ConfigureKeyVault(keyVaultOptions => {
                            keyVaultOptions.SetCredential(azureCredential);
                        })
                        .ConfigureRefresh(refreshOptions => {
                            refreshOptions.Register(key: "TestApp:Settings:Message", label: LabelFilter.Null, refreshAll: true);
                            refreshOptions.SetCacheExpiration(TimeSpan.FromSeconds(30));
                        });

                    ConfigurationRefresher = appConfigOptions.GetRefresher();
                });
            }
        }
    }
}