using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace lib.extensions
{
    public static class ChannelExtensions
    {

        public static int ChannelCapacity { get; } = 100;

        public static BoundedChannelOptions GetSingleProducerChannelOptions() {
            return new BoundedChannelOptions(ChannelCapacity) {
                SingleReader = true,
                SingleWriter = false,
                AllowSynchronousContinuations = false,
                FullMode = BoundedChannelFullMode.DropOldest
            };
        }

        public static void DroppedMessage<T>(T dropped) {
            Log.Error($"Channel capacity (size: {ChannelCapacity}) exceeded for '{typeof(T).ToString()}'");
        }

         public static IServiceCollection AddSingleConsumerChannel<T>(this IServiceCollection services)
        {
            services.AddSingleton<Channel<T>>(
                Channel.CreateBounded<T>(
                    new BoundedChannelOptions(ChannelCapacity) {
                        SingleReader = false,
                        SingleWriter = true,
                        AllowSynchronousContinuations = false,
                        FullMode = BoundedChannelFullMode.DropOldest
                    },
                    static void (T dropped) => DroppedMessage(dropped)
                )
            );
            services.AddReaderAndWriter<T>();
            return services;
        }

        private static void AddReaderAndWriter<T>(this IServiceCollection services)
        {
            services.AddSingleton<ChannelReader<T>>(sp => sp.GetRequiredService<Channel<T>>().Reader);
            services.AddSingleton<ChannelWriter<T>>(sp => sp.GetRequiredService<Channel<T>>().Writer);
        }
    }
}