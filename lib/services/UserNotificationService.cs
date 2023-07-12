using System;
using System.Threading.Channels;
using Microsoft.EntityFrameworkCore;
using lib.models;
using lib.models.db;
using lib.models.mqtt;
using System.Text.Json;
using lib.extensions;
using System.Threading.Tasks;
using System.Linq;

namespace lib.services
{
    public interface IUserNotificationService {
        Task Send(
            Guid UserId,
            string Title,
            string Body,
            UserNotificationLevel Level = UserNotificationLevel.INFO,
            Guid? WorkspaceId = null
        );
    }

    public class UserNotificationService : IUserNotificationService
    {
        private ChannelWriter<MqttPublishMessage> _publishWriter;
        private IDbContextFactory<CvopsDbContext> _dbContextFactory;
        public UserNotificationService(
            ChannelWriter<MqttPublishMessage> publishWriter,
            IDbContextFactory<CvopsDbContext> dbContextFactory
        ) {
            _publishWriter = publishWriter;
            _dbContextFactory = dbContextFactory;
        }

        public async Task Send(
            Guid userId, 
            string title, 
            string body,
            UserNotificationLevel level = UserNotificationLevel.INFO,
            Guid? workspaceId = null
        ) {
            if (workspaceId == null) {
                using ( var context = _dbContextFactory.CreateDbContext()) {
                    var user = await context.Users
                                        .Where(u => u.Id == userId || u.JwtSubject == userId.ToString())
                                        .FirstAsync();
                    if (user == null) throw new Exception($"user not found: {userId}");
                    workspaceId = user.DefaultWorkspaceId;
                }
            }

            var eventData = new UserNotificationPayload() {
                NotificationId = Guid.NewGuid(),
                UserId = userId,
                Title = title,
                Body = body,
                Level = UserNotificationLevel.INFO
            };

            var platformEvent = new PlatformEvent() {
                WorkspaceId = (Guid)workspaceId,
                UserId = userId,
                EventType = PlatformEventTypes.UserNotification,
                EventData = JsonSerializer.SerializeToDocument(eventData, LocalJsonOptions.DefaultOptions)
            };

            var message =  new MqttPublishMessage() {
                Topic = $"events/user/{userId}/notification",
                Payload = platformEvent
            };

            await _publishWriter.WriteAsync(message).ConfigureAwait(false);
        }
    }
}