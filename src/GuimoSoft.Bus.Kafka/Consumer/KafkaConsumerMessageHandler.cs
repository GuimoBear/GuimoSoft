using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GuimoSoft.Bus.Kafka.Consumer
{
    public sealed class KafkaConsumerEventHandler : IHostedService, IDisposable
    {
        private readonly Lazy<CancellationTokenSource> _lazyCts;
        private readonly IKafkaEventConsumerManager _manager;

        private Task _kafkaListenner = Task.CompletedTask;

        public KafkaConsumerEventHandler(IKafkaEventConsumerManager manager)
        {
            _manager = manager ?? throw new ArgumentNullException(nameof(manager));
            _lazyCts = new Lazy<CancellationTokenSource>(() => new CancellationTokenSource());
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _kafkaListenner = Task.Factory.StartNew(() => _manager.StartConsumers(_lazyCts.Value.Token), cancellationToken);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Dispose();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            if (_lazyCts.IsValueCreated && !_lazyCts.Value.IsCancellationRequested)
            {
                _lazyCts.Value.Cancel();
                _kafkaListenner.Wait();
            }
        }
    }
}
