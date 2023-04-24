using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using lib.models;

namespace lib.extensions
{

    public static class WebApplicationExtensions
    {
        public static void Initialize(this WebApplication app)
        {
            app.CheckDatabaseConnection();
        }

        public static void CheckDatabaseConnection(this WebApplication app)
        {
            var dbContext = app.Services.GetRequiredService<CvopsDbContext>();
            dbContext.Database.CanConnect();
        }

        public static void SetupMQTT(this WebApplication app)
        {
            // var config = app.Services.GetRequiredService<AppConfiguration>();
            // var mqttService = app.Services.GetRequiredService<MQTTService>();
            // mqttService.Connect(config.MQTT.Uri, config.MQTT.Port, config.MQTT.AdminUsername, config.MQTT.AdminPassword);
        }
    }

}