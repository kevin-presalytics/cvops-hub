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
            var workingDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            builder.Configuration.AddYamlFile(Path.Join(workingDir, "appsettings.default.yaml"), optional: false, reloadOnChange: true);
            builder.Configuration.AddYamlFile(Path.Join(workingDir, "appsettings.yaml"), optional: true, reloadOnChange: true);
            builder.Configuration.AddYamlFile(Path.Join(workingDir, "appsettings.local.yaml"), optional: true, reloadOnChange: true);
            builder.Configuration.AddEnvironmentVariables();
            AppConfiguration appConfiguration = new AppConfiguration();
            builder.Configuration.Bind(appConfiguration);
            builder.Services.AddSingleton<AppConfiguration>(appConfiguration);
            return appConfiguration;
        }
    }
}