using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Benchmark.Bus.Fakes.Kafka;
using GuimoSoft.Bus.Kafka.Consumer;

namespace GuimoSoft.Benchmark.Bus.Fakes
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection InjectInMemoryKafka(this IServiceCollection services)
        {
            var sd = services.FirstOrDefault(sd => sd.ServiceType == typeof(IKafkaConsumerBuilder));
            if (sd is not null)
                services.Remove(sd);

            services.AddSingleton(InMemoryBroker.Consumer);
            services.AddSingleton(InMemoryBroker.Producer);
            services.AddTransient<IMessageProducer, InMemoryMessageProducer>();
            services.AddTransient<IKafkaConsumerBuilder, InMemoryKafkaConsumerBuilder>();
            services.AddTransient<IMessageProducer, InMemoryMessageProducer>();

            return services;
        }
    }
}
