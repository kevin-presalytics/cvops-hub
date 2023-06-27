using System.Threading.Tasks;
using lib.models.db;
using Serilog;
using System;
using Microsoft.Extensions.Hosting;

namespace lib.services.mqtt.listeners
{
    public class DeviceDataTopicListener : MqttTopicListenerWithTypedPayload<InferenceResult>
    {
        private IInferenceResultService _inferenceResultService;
        public DeviceDataTopicListener(
            IInferenceResultService inferenceResultService, 
            IHubMqttClient mqttClient,
            IHostApplicationLifetime applicationLifetime,
            ILogger logger) 
            : base(mqttClient, applicationLifetime, logger)
        {
            _inferenceResultService = inferenceResultService;
        }
        public override string TopicFilter { get => "device/+/data"; }
        public event EventHandler<InferenceResult>? InferenceResultEvent;

        public override async Task HandlePayload(InferenceResult payload)
        {
            await _inferenceResultService.Write(payload);
            InferenceResultEvent?.Invoke(this, payload);
        }
    }
}