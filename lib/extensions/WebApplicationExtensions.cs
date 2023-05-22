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
    }

}