using InfluxDB.Client;
using lib.models.configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;


namespace lib.services
{
    public interface ITimeSeriesDBService
    {
        void Write(object data, string bucket, Guid workspaceId);
        Task<List<object>> Query(string fluxQuery, Guid workspaceId);

    }

    public class InfluxDBService : ITimeSeriesDBService {

        private InfluxDBClient _client {get;}
        public InfluxDBService(
            AppConfiguration appConfiguration
        ) {
            _client = new InfluxDBClient(
                appConfiguration.TimeSeriesDB.Host, 
                appConfiguration.TimeSeriesDB.Username, 
                appConfiguration.TimeSeriesDB.Password
            );
        }

        public void Write(object data, string bucket, Guid workspaceId) {
            using (var writer = _client.GetWriteApi()) {
                writer.WriteMeasurement(data, InfluxDB.Client.Api.Domain.WritePrecision.Ns, bucket, workspaceId.ToString());
            }
        }

        public async Task<List<object>> Query(string fluxQuery, Guid workspaceId) {
            // TODO: I'd like a cleaner output this List<object>, ideally something that guarantees the type of the object
            // Need to do some experimentation with real inference data to figure this out.
            var fluxTables = await _client.GetQueryApi().QueryAsync(fluxQuery, workspaceId.ToString());
            return fluxTables.SelectMany(t => t.Records).ToList<object>();
            
        }


    }
}