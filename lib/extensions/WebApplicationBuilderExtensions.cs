using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using lib.models.configuration;
using System.Reflection;
using System.IO;
using Utility.Extensions.Configuration.Yaml;

namespace lib.extensions
{
    public static class WebApplicationBuilderExtensions
    {
        public static AppConfiguration AddAppConfiguration(this WebApplicationBuilder builder)
        {
            var appConfiguration = builder.Configuration.Configure();
            builder.Services.AddSingleton<AppConfiguration>(appConfiguration);
            return appConfiguration;
        }

        public static AppConfiguration Configure(this ConfigurationManager configManager)

        {
            var workingDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            configManager.AddYamlFile(Path.Join(workingDir, "appsettings.default.yaml"), optional: false, reloadOnChange: true);
            configManager.AddYamlFile(Path.Join(workingDir, "appsettings.yaml"), optional: true, reloadOnChange: true);
            configManager.AddYamlFile(Path.Join(workingDir, "appsettings.local.yaml"), optional: true, reloadOnChange: true);
            configManager.AddEnvironmentVariables();
            AppConfiguration appConfiguration = new AppConfiguration();
            configManager.Bind(appConfiguration);
            return appConfiguration;
        }
    
    }
}