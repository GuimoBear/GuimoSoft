using BenchmarkDotNet.Attributes;
using Confluent.Kafka;
using GuimoSoft.Benchmark.Bus.Fakes;
using GuimoSoft.Benchmark.Bus.Handlers.Benchmark;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Bus.Core.Internal.Middlewares;
using GuimoSoft.Bus.Kafka;
using GuimoSoft.Bus.Kafka.Consumer;
using GuimoSoft.Core.Serialization;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Threading.Tasks;

namespace GuimoSoft.Benchmark.Bus
{
    [Description("Consuming and send via EventDispatcherMiddleware")]
    public class EventDispatcherMiddlewareBenchmark : BenchmarkBase
    {
        private IConsumer<string, byte[]> Consumer { get; set; }

        [GlobalSetup]
        public override Task GlobalSetupAsync()
        {
            var services = new ServiceCollection();
            services
                .AddKafkaConsumer(configurer => 
                    configurer
                        .FromServer(_ => { })
                        .AddAnotherAssemblies(typeof(BenchmarkBase).Assembly))
                .InjectInMemoryKafka()
                .AddSingleton<BenchmarkEventHandler>()
                .AddSingleton(new EventDispatcherMiddleware<BenchmarkEvent>());

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
            var eventContext = new ConsumeContext<BenchmarkEvent>(@event, Services, new ConsumeInformations(BusName.Kafka, ServerName.Default, BenchmarkEvent.TOPIC_NAME), CancellationTokenSource.Token);
            var mediator = Services.GetRequiredService<EventDispatcherMiddleware<BenchmarkEvent>>();
            await mediator.InvokeAsync(eventContext, () => Task.CompletedTask);
            WaitId();
        }
    }
}
