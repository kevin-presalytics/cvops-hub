using lib.models.db;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using lib.models;
using Serilog;

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
        private readonly ILogger _logger;


        public PlatformEventService(IDbContextFactory<CvopsDbContext> contextFactory, ILogger logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        public async Task Write(PlatformEvent dataPoint)
        {
            _logger.Debug("Writing platform event for workspace {workspaceId}");
            try {
                using (var _context = _contextFactory.CreateDbContext())
                {
                    Guid workspaceId = dataPoint.WorkspaceId;
                    if (dataPoint.WorkspaceId == Guid.Empty)
                    {
                        if (dataPoint.DeviceId != null && dataPoint.DeviceId != Guid.Empty)
                        {
                            workspaceId = await _context.Devices
                                .Where(d => d.Id == dataPoint.DeviceId)
                                .Select(d => d.WorkspaceId)
                                .FirstOrDefaultAsync();
                        } else {
                            throw new Exception("Cannot write a user platform event without workspace id");
                        }
                    }
                    int eventType = (int)dataPoint.EventType;

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
                            {dataPoint.DeviceId},
                            {dataPoint.UserId},
                            {dataPoint.Time},
                            {dataPoint.EventType},
                            {dataPoint.EventData}
                        )");
                }
            } catch (Exception e) {
                _logger.Error(e, "Error writing platform event");
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