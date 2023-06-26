using Microsoft.Extensions.DependencyInjection;
using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Json;
using MvcJsonOptions = Microsoft.AspNetCore.Mvc.JsonOptions;
using lib.models;
using lib.models.mqtt;

namespace lib.extensions
{
    public static class LocalJsonOptions
    {
        public static void ConfigureJson(this IMvcBuilder controllerOptions)
        {
            controllerOptions.AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
                options.JsonSerializerOptions.Converters.Add(new PaginatedListJsonConverterFactory());
            });
        }

        public static void ConfigureJson(this IServiceCollection services)
        {
            services.Configure<JsonOptions>(o => o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
            services.Configure<MvcJsonOptions>(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        }

        public static JsonSerializerOptions GetOptions() {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new JsonStringEnumConverter());
            options.Converters.Add(new PaginatedListJsonConverterFactory());
            options.Converters.Add(new MqttDataMessageJsonConverter());
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            return options;
        }

        

    }
}