using lib.models.db;
using lib.models;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text.Json;
using System.Globalization;

namespace lib.services
{
    public interface IPlatformEventService
    {
        Task Write(PlatformEvent dataPoint);

        Task<List<PlatformEvent>> Query(
            Guid workspaceId, 
            Guid? deviceId,
            Guid? userId,
            DateTimeOffset? start, 
            DateTimeOffset? end, 
            int? limit
        );
    }

    public class PlatformEventService : IPlatformEventService
    {
        private readonly IDbContextFactory<CvopsDbContext> _contextFactory;


        public PlatformEventService(IDbContextFactory<CvopsDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task Write(PlatformEvent dataPoint)
        {
            string workspaceId = dataPoint.WorkspaceId.ToString();
            # pragma warning disable CS8600
            string deviceId = dataPoint.DeviceId == null ? "NULL" : dataPoint.DeviceId.ToString();
            string userId = dataPoint.UserId == null ? "NULL" : dataPoint.UserId.ToString();
            # pragma warning restore CS8600
            string time = dataPoint.Time.DateTime.ToString("o", CultureInfo.InvariantCulture);
            int eventType = (int)dataPoint.EventType;
            string eventData = JsonSerializer.Serialize(dataPoint.EventData);

            using (var _context = _contextFactory.CreateDbContext())
            {

                await _context.Database.ExecuteSqlInterpolatedAsync($@"
                    INSERT INTO platform_events (
                        workspace_id,
                        device_id,
                        user_id,
                        time,
                        event_type,
                        event_data
                    ) VALUES (
                        {workspaceId},
                        {deviceId},
                        {userId},
                        {time}::timestamp,
                        {eventType},
                        {eventData}
                    )");
            }
        }

        public async Task<List<PlatformEvent>> Query(
            Guid workspaceId, 
            Guid? deviceId,
            Guid? userId,
            DateTimeOffset? start, 
            DateTimeOffset? end, 
            int? limit
        )
        {
            using (var _context = _contextFactory.CreateDbContext())
            {
                var query = _context.PlatformEvents
                    .Where(x => x.WorkspaceId == workspaceId);

                if (deviceId != null) query = query.Where(x => x.DeviceId == deviceId);
                if (start != null) query = query.Where(x => x.Time >= start);
                if (end != null) query = query.Where(x => x.Time <= end);
                if (limit != null) query = query.Take(limit.Value);

                return await query.ToListAsync();
            }
        }
    }
}