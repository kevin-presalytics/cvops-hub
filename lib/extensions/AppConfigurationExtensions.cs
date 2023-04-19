using lib.models.configuration;

namespace lib.extensions
{
    public static class AppConfigurationExtensions
    {
        public static string GetPostgresqlConnectionString(this AppConfiguration config)
        {
            return $"Host={config.Postgresql.Host};Port={config.Postgresql.Port};Database={config.Postgresql.Database};Username={config.Postgresql.Username};Password={config.Postgresql.Password};SslMode={config.Postgresql.SslMode}";
        }
    }
}