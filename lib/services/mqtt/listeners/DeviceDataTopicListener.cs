using System.Threading.Tasks;
using lib.models.db;
using Serilog;
using System.Threading.Channels;
using lib.models.mqtt;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Hosting;
using System.Linq;
using System.Threading;

namespace lib.services.mqtt.listeners
{
    public class DeviceDataTopicListener : MqttTopicListenerWithTypedPayload<InferenceResult>
    {
        private IInferenceResultService _inferenceResultService;
        private List<ChannelWriter<InferenceResult>> _inferenceResultWriters = new List<ChannelWriter<InferenceResult>>();
        private IServiceProvider _serviceProvider;
        public DeviceDataTopicListener(
            IInferenceResultService inferenceResultService,
            ChannelWriter<MqttSubscriptionMessage> subscriptionWriter,
            ILogger logger,
            IServiceProvider serviceProvider) 
            : base(logger, subscriptionWriter)
        {
            _inferenceResultService = inferenceResultService;
            _serviceProvider = serviceProvider;
        }
        public override string TopicFilter { get => "$share/g/device/+/data"; }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            DiscoverInferenceResultWriters();
            return base.ExecuteAsync(stoppingToken);
        }

        public override async Task HandlePayload(InferenceResult payload)
        {
            await _inferenceResultService.Write(payload);
            await DispatchInferenceResults(payload);
        }

        private void DiscoverInferenceResultWriters()
        {
            _inferenceResultWriters = _serviceProvider
                                        .GetServices<IHostedService>()
                                        .Where(s => s is IChannelOwner<InferenceResult>)
                                        .Select(s => ((IChannelOwner<InferenceResult>)s).ChannelWriter)
                                        .ToList();
            _logger.Debug("Discovered {count} inference result channels to writer to.", _inferenceResultWriters.Count);
        }

        private async Task DispatchInferenceResults(InferenceResult payload)
        {
            foreach (var writer in _inferenceResultWriters)
            {
                await writer.WriteAsync(payload);
            }
        }
    }

    public static class DeviceDataTopicListenerExtensions
    {
        public static IServiceCollection AddDeviceDataTopicListener(this IServiceCollection services)
        {
            services.AddHostedService<DeviceDataTopicListener>();
            return services;
        }
    }
}