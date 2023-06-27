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
    public interface IInferenceResultService
    {
        Task Write(InferenceResult dataPoint);

        Task<List<InferenceResult>> Query(
            Guid workspaceId, 
            Guid? deviceId, 
            DateTimeOffset? start, 
            DateTimeOffset? end, 
            int? limit
        );
    }

    public class InferenceResultService : IInferenceResultService
    {
        private readonly CvopsDbContext _context;

        public InferenceResultService(CvopsDbContext context)
        {
            _context = context;
        }

        public async Task Write(InferenceResult dataPoint)
        {
            string workspaceId = dataPoint.WorkspaceId.ToString();
            string deviceId = dataPoint.DeviceId.ToString();
            string time = dataPoint.Time.DateTime.ToString("o", CultureInfo.InvariantCulture);
            int resultType = (int)dataPoint.ResultType;
            string boxes = dataPoint.Boxes == null ? "NULL": JsonSerializer.Serialize(dataPoint.Boxes);
            string meshes = dataPoint.Meshes == null ? "NULL" : JsonSerializer.Serialize(dataPoint.Meshes);
            string labels = dataPoint.Labels == null ? "NULL" : JsonSerializer.Serialize(dataPoint.Labels);

            await _context.Database.ExecuteSqlInterpolatedAsync($@"
                INSERT INTO inference_results (
                    workspace_id,
                    device_id,
                    time,
                    result_type,
                    boxes,
                    meshes,
                    labels
                ) VALUES (
                    {workspaceId},
                    {deviceId},
                    {time}::timestamp,
                    {resultType},
                    {boxes},
                    {meshes},
                    {labels}
                )");
        }

        public async Task<List<InferenceResult>> Query(
            Guid workspaceId, 
            Guid? deviceId, 
            DateTimeOffset? start, 
            DateTimeOffset? end, 
            int? limit
        )
        {
            var query = _context.InferenceResults
                .Where(x => x.WorkspaceId == workspaceId);

            if (deviceId != null) query = query.Where(x => x.DeviceId == deviceId);
            if (start != null) query = query.Where(x => x.Time >= start);
            if (end != null) query = query.Where(x => x.Time <= end);
            if (limit != null) query = query.Take(limit.Value);

            return await query.ToListAsync();
        }

        public Task<List<InferenceResult>> Read(Guid workspaceId, Guid? deviceId, DateTimeOffset? start, DateTimeOffset? end, int? limit)
        {
            throw new NotImplementedException();
        }
    }
}