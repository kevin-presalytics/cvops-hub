using MQTTnet;
using lib.models.mqtt;
using System.Threading.Tasks;
using System.Threading;
using System.Threading.Channels;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;

namespace lib.services.mqtt.queue
{
    public interface IMqttTopicQueue<T> where T : IMqttPayload
    {
        ValueTask EnqueueAsync(MqttApplicationMessage message);
        ValueTask<T> DequeueAsync(CancellationToken cancellationToken);
    }

    public class MqttTopicQueue<T> : IMqttTopicQueue<T> where T : IMqttPayload
    {
        private readonly Channel<T> _queue;
        public MqttTopicQueue(int capacity = 1000)
        {
            var options = new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait
            };
            _queue = Channel.CreateBounded<T>(options);
        }
        public virtual async ValueTask EnqueueAsync(MqttApplicationMessage message) {
            
            await _queue.Writer.WriteAsync(message.AsMqttPayload<T>());
        }
        public virtual async ValueTask<T> DequeueAsync(CancellationToken cancellationToken) {
            return await _queue.Reader.ReadAsync(cancellationToken);
        }
    }

    public abstract class MqttTopicWorker<T> : BackgroundService where T : IMqttPayload
    {
        protected readonly IMqttTopicQueue<T> _queue;
        private readonly ILogger _logger;
        public MqttTopicWorker(IMqttTopicQueue<T> queue, ILogger logger)
        {
            _queue = queue;
            _logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.Debug($"Starting hosted service: {this.GetType().Name}...");
            while (!stoppingToken.IsCancellationRequested) {
                try {
                    T payload = await _queue.DequeueAsync(stoppingToken);
                    _logger.Debug("Received message: ", payload.ToString());
                    await HandlePayload(payload);
                } catch (Exception ex) {
                    _logger.Error(ex, "Error processing message");
                }
            }
        }
        protected abstract Task HandlePayload(T payload);
    }
}