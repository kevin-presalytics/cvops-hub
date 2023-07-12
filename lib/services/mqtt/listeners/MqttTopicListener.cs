using System.Threading.Tasks;
using System.Threading;
using MQTTnet;
using lib.models.mqtt;
using Microsoft.Extensions.Hosting;
using System;
using Serilog;
using System.Threading.Channels;

namespace lib.services.mqtt
{

    public interface IMqttTopicListener 
    {
        string TopicFilter {get;}
        Task HandleMessage(MqttApplicationMessage message);

        ChannelWriter<MqttApplicationMessage> MessageWriter {get;}
    }

    public abstract class MqttTopicListener : BackgroundService, IMqttTopicListener, IChannelOwner<MqttApplicationMessage>
    {
        public abstract string TopicFilter { get;}
        protected ILogger _logger;
        protected Channel<MqttApplicationMessage> _messageChannel;
        protected ChannelWriter<MqttSubscriptionMessage> _subscriptionWriter;
        static string _sharedPrefix = "$share/g/";

        public MqttTopicListener(
            ILogger logger,
            ChannelWriter<MqttSubscriptionMessage> subscriptionWriter
        ) {
            _logger = logger;
            _subscriptionWriter = subscriptionWriter;
            _messageChannel = Channel.CreateUnbounded<MqttApplicationMessage>();
        }

        public ChannelReader<MqttApplicationMessage> ChannelReader {
            get {
                return _messageChannel.Reader;
            }
        }

        public ChannelWriter<MqttApplicationMessage> ChannelWriter {
            get {
                return _messageChannel.Writer;
            }
        }

        protected async Task HandleApplicationMessages(CancellationToken stoppingToken)
        {
            while (await _messageChannel.Reader.WaitToReadAsync(stoppingToken))
            {
                while(_messageChannel.Reader.TryRead(out MqttApplicationMessage? message))
                {
                    if (message != null)
                    {
                        await RouteMessage(message);
                    }

                }
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await SubscribeToTopic();
            await HandleApplicationMessages(stoppingToken);
            await OnStopping();
        }


        private async Task SubscribeToTopic()
        {
            _logger.Information("Subscribing to topic {topic} for {typeName}", TopicFilter, this.GetType().Name);
            try {
                var message = new MqttSubscriptionMessage(this.TopicFilter, false);
                await _subscriptionWriter.WriteAsync(message);
            } catch (Exception ex ) {
                _logger.Error(ex, "Error subscribing to topic {topic}", TopicFilter);
            }
            
        }
        // Need to trim shared prefix to topic since MqttNet's topic comparer doesn't handle $shared prefixes
        private string _topicComparer {
            get {
                return TopicFilter.Replace(_sharedPrefix, "");
            }
        }

        public ChannelWriter<MqttApplicationMessage> MessageWriter => throw new NotImplementedException();

        private async Task RouteMessage(MqttApplicationMessage message)
        {
            MqttTopicFilterCompareResult compareResult = MqttTopicFilterComparer.Compare(message.Topic, _topicComparer);
            _logger.Debug("Message {topic} compared to {topicComparer} with result {compareResult}", message.Topic, _topicComparer, compareResult.ToString());
            if (compareResult == MqttTopicFilterCompareResult.IsMatch) {
                await HandleMessage(message);
            }
        }

        public abstract Task HandleMessage(MqttApplicationMessage message);

        private async Task OnStopping()
        {
            await UnsubscribeFromTopic();
        }

        private async Task UnsubscribeFromTopic()
        {
            var message = new MqttSubscriptionMessage(this.TopicFilter, true);
            await _subscriptionWriter.WriteAsync(message);
        }
    }

    public abstract class MqttTopicListenerWithTypedPayload<T> : MqttTopicListener where T : IMqttPayload
    {
        public MqttTopicListenerWithTypedPayload(
            ILogger logger,
            ChannelWriter<MqttSubscriptionMessage> subscriptionWriter
        ) : base(logger, subscriptionWriter) {}

        public abstract Task HandlePayload(T payload);

        public override async Task HandleMessage(MqttApplicationMessage message)
        {
            T payload = message.AsMqttPayload<T>();
            await HandlePayload(payload);
        }
    }

    

}