using System.Threading.Tasks;
using lib.models.db;
using Serilog;
using System.Threading.Channels;
using lib.models.mqtt;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Microsoft.Extensions.Hosting;
using System.Threading;

namespace lib.services.mqtt.listeners
{
    public class PlatformEventTopicListener : MqttTopicListenerWithTypedPayload<PlatformEvent>
    {
        private IPlatformEventService _platformEventService;
        private IServiceProvider _serviceProvider;
        private List<ChannelWriter<PlatformEvent>> _platformEventWriters = new List<ChannelWriter<PlatformEvent>>();
        
        public PlatformEventTopicListener(
            IPlatformEventService platformEventService,
            ILogger logger,
            ChannelWriter<MqttSubscriptionMessage> subscriptionWriter,
            IServiceProvider serviceProvider 
        ) : base(logger, subscriptionWriter) {
            _platformEventService = platformEventService;
            _serviceProvider = serviceProvider;
        }
        public override string TopicFilter { get => "$share/g/events/#"; }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            DiscoverPlatformEventWriters();
            return base.ExecuteAsync(stoppingToken);
        }

        public override async Task HandlePayload(PlatformEvent platformEvent)
        {
            await _platformEventService.Write(platformEvent);
            await DispatchPlatformEvents(platformEvent);
        }

        private async Task DispatchPlatformEvents(PlatformEvent platformEvent)
        {
            foreach (var writer in _platformEventWriters)
            {
                await writer.WriteAsync(platformEvent).ConfigureAwait(false);
            }
        }

        private void DiscoverPlatformEventWriters() {
            _platformEventWriters = _serviceProvider
                                        .GetServices<IHostedService>()
                                        .Where(s => s is IChannelOwner<PlatformEvent>)
                                        .Select(s => ((IChannelOwner<PlatformEvent>)s).ChannelWriter)
                                        .ToList();
            _logger.Debug("Discovered {count} platform event channels to writer to.", _platformEventWriters.Count);
        }
    }

    public static class PlatformEventTopicListenerExtensions
    {
        public static IServiceCollection AddPlatformEventTopicListener(this IServiceCollection services)
        {
            services.AddHostedService<PlatformEventTopicListener>();
            return services;
        }
    }
}