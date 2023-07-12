using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using System.Threading;
using Serilog;
using System;
using lib.extensions;

namespace lib.services
{
    public interface IChannelListener<T> : IChannelOwner<T>
    {
        Task HandleMessage(T message);
    }

    public abstract class ChannelListener<T> : BackgroundService, IChannelListener<T>
    {
        protected ILogger _logger;
        protected Channel<T> _messageChannel = Channel.CreateBounded<T>(
            ChannelExtensions.GetSingleProducerChannelOptions(),
            static void (T dropped) => ChannelExtensions.DroppedMessage(dropped)
        );

        public ChannelReader<T> ChannelReader {
            get {
                return _messageChannel.Reader;
            }
        }

        public ChannelWriter<T> ChannelWriter {
            get {
                return _messageChannel.Writer;
            }
        }

        public ChannelListener(
            ILogger logger
        ) {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.Information($"Starting channel listener {this.GetType().Name}");
            while (await ChannelReader.WaitToReadAsync(stoppingToken))
            {
                _logger.Debug($"{this.GetType().Name} Channel has message");
                while(ChannelReader.TryRead(out T? message))
                {
                    _logger.Debug($"{this.GetType().Name} Channel message read.");
                    if (message != null)
                    {
                        try {
                            _logger.Debug("Handling message in channel listener subclass implementation.");
                            await HandleMessage(message);
                        } catch (Exception ex) {
                            _logger.Error(ex, $"Failed to handle message from channel {typeof(T).ToString()} in class {this.GetType().Name}");
                        }
                        
                    } else {
                        _logger.Error($"Failed to read message from channel {typeof(T).ToString()} in class {this.GetType().Name}");
                    }

                }
            }
        }

        public abstract Task HandleMessage(T message);
    }
}