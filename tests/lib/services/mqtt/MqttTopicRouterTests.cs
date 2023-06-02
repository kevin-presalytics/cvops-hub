using Serilog;
using lib.services.mqtt;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace tests.lib.services.mqtt
{
    public class MqttTopicRouterTests
    {
        private ILogger logger = new Mock<ILogger>().Object;

        private interface IDummyService {}
        private class DummyService : IDummyService {}

        [Fact]
        public void DiscoverListeners_WithMixedServices_FindsOnlyListeners()
        {
            var mockListener1 = new Mock<IMqttTopicListener>();
            mockListener1.Setup(l => l.TopicFilter).Returns("test/topic1");
            var mockListener2 = new Mock<IMqttTopicListener>();
            mockListener2.Setup(l => l.TopicFilter).Returns("test/topic2");

            var services = new ServiceCollection();
            services.AddSingleton<ILogger>(logger);
            services.AddTransient<IMqttTopicListener>(sp => mockListener1.Object);
            services.AddTransient<IMqttTopicListener>(sp => mockListener2.Object);
            services.AddTransient<IDummyService, DummyService>();
            services.AddSingleton<IMqttTopicRouter, MqttTopicRouter>();

            var serviceProvider = services.BuildServiceProvider();

            var router = serviceProvider.GetRequiredService<IMqttTopicRouter>();

            // Act
            router.DiscoverTopicListeners();

            // Assert
            Assert.Equal(2, router.TopicListeners.Count);
        }
    }
}