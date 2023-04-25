// namespace lib.services.mqtt
// {
//     public interface IMqttMessageHandler
//     {
//         string Topic { get; }
//         void Initialize();
//         void HandleMessage<T>(string topic, string message) where T : IMqttTopicMessage
//     }
//     public abstract class AbstractMqttMessageHandler : IMqttMessageHandler
//     {
//         IHubMQTTClient _hubMQTTClient;
//         public AbstractMqttMessageHandler(IHubMQTTClient hubMQTTClient)
//         {
//             _hubMQTTClient = hubMQTTClient;
//         }

//         public abstract string Topic { get; }
//         public abstract void HandleMessage(string topic, string message);
//         public abstract void Initialize();
//     }
// }