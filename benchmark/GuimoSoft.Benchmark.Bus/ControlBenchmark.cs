using BenchmarkDotNet.Attributes;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Benchmark.Bus.Fakes;
using GuimoSoft.Benchmark.Bus.Handlers.Benchmark;
using GuimoSoft.Bus.Kafka.Consumer;
using GuimoSoft.Core.Serialization;

namespace GuimoSoft.Benchmark.Bus
{
    [Description("Consuming directly from IConsumer<string, byte[]>")]
    public class ControlBenchmark : BenchmarkBase
    {
        private IConsumer<string, byte[]> Consumer { get; set; }

        [GlobalSetup]
        public override Task GlobalSetupAsync()
        {
            var services = new ServiceCollection();
            services
                .InjectInMemoryKafka();

            Services = services.BuildServiceProvider(true);

            Producer = Services.GetRequiredService<IEventBus>();
            var consumerBuilder = Services.GetRequiredService<IKafkaConsumerBuilder>();

            Consumer = consumerBuilder.Build(ServerName.Default);

            Consumer.Subscribe(BenchmarkEvent.TOPIC_NAME);

            return Task.CompletedTask;
        }

        [GlobalCleanup]
        public override Task GlobalCleanupAsync()
        {
            Consumer.Unsubscribe();
            Consumer.Close();
            Services.Dispose();
            return Task.CompletedTask;
        }

        [Benchmark(Description = "produce and consume event")]
        public override async Task ProduceAndConsume()
        {
            await Produce();
            var result = Consumer.Consume(CancellationTokenSource.Token);
            var @event = JsonEventSerializer.Instance.Deserialize(typeof(BenchmarkEvent), result.Message.Value) as BenchmarkEvent;
            BenchmarkContext.Add(@event.Id);
            WaitId();
        }
    }
}
