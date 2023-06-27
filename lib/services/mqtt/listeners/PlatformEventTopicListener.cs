using System.Threading.Tasks;
using lib.models.db;
using Serilog;
using System;
using Microsoft.Extensions.Hosting;

namespace lib.services.mqtt.listeners
{
    public class PlatformEventTopicListener : MqttTopicListenerWithTypedPayload<PlatformEvent>
    {
        private IPlatformEventService _platformEventService;
        public PlatformEventTopicListener(
            IPlatformEventService platformEventService,
            IHubMqttClient mqttClient,
            IHostApplicationLifetime applicationLifetime,
            ILogger logger) : base(mqttClient, applicationLifetime, logger) {
            _platformEventService = platformEventService;
        }
        public override string TopicFilter { get => "+/+/events"; }

        public event EventHandler<PlatformEvent>? DeviceRegisteredEvent;
        public event EventHandler<PlatformEvent>? DeviceUnregisteredEvent;
        public event EventHandler<PlatformEvent>? UserLoginEvent;
        public event EventHandler<PlatformEvent>? UserLogoutEvent;

        public override async Task HandlePayload(PlatformEvent platformEvent)
        {
            await _platformEventService.Write(platformEvent);

            switch (platformEvent.EventType) {
                case PlatformEventTypes.DeviceRegistered:
                    DeviceRegisteredEvent?.Invoke(this, platformEvent);
                    break;
                case PlatformEventTypes.DeviceUnregistered:
                    DeviceUnregisteredEvent?.Invoke(this, platformEvent);
                    break;
                case PlatformEventTypes.UserLogin:
                    UserLoginEvent?.Invoke(this, platformEvent);
                    break;
                case PlatformEventTypes.UserLogout:
                    UserLogoutEvent?.Invoke(this, platformEvent);
                    break;
                default:
                    _logger.Warning("PlatformEventListener received unknown data type: {DataType}", platformEvent.EventType);
                    break;
            }
        }
    }
}