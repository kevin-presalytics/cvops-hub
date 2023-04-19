using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using System.Threading;

namespace mqtt_controller
{

    public class Worker : BackgroundService
    {

        public Worker()
        {

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}