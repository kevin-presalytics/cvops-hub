using lib.models.mqtt;
using Serilog;
using System.Text.Json;
using System.Threading.Tasks;
using lib.models.db;
using lib.services.factories;
using lib.services;


namespace mqtt.workers
{
    public class UserEventsWorker : ChannelListener<PlatformEvent>
    {
        private IScopedServiceFactory<IUserService> _userServiceFactory;
        private IUserNotificationService _userNotificationService;
        public UserEventsWorker(
            ILogger logger,
            IScopedServiceFactory<IUserService> userServiceFactory,
            IUserNotificationService userNotificationService
        ) : base(logger)
        {
            _userServiceFactory = userServiceFactory;
            _userNotificationService = userNotificationService;
        } 

        public override Task HandleMessage(PlatformEvent platformEvent)
        {
            switch (platformEvent.EventType) {
                case PlatformEventTypes.UserLogin:
                    SendWelcomeMessage(platformEvent);
                    break;
                default:
                    break;
            }
            return Task.CompletedTask;
        }

        public void SendWelcomeMessage(PlatformEvent platformEvent)
        {
            UserLoginPayload? payload = JsonSerializer.Deserialize<UserLoginPayload>(platformEvent.EventData);
            if (payload != null) {
                 _userNotificationService.Send(
                    payload.UserId,
                    $"Welcome {payload.Username}!",
                    $"You have successfully logged in.  Get started by a adding a new device"
                );
            }
           
        }
    }
}