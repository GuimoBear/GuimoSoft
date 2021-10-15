using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using GuimoSoft.Bus.Abstractions;
using GuimoSoft.Benchmark.Bus.Handlers.Benchmark;

namespace GuimoSoft.Benchmark.Bus
{
    public abstract class BenchmarkBase
    {
        protected ServiceProvider Services { get; set; }
        protected IEventBus Producer { get; set; }

        private Guid _currentId;

        protected readonly CancellationTokenSource CancellationTokenSource = new();

        public abstract Task GlobalSetupAsync();

        public abstract Task GlobalCleanupAsync();

        public abstract Task ProduceAndConsume();

        protected async Task Produce()
            => await Producer.Publish(BenchmarkEvent.TOPIC_NAME, new BenchmarkEvent(_currentId = Guid.NewGuid()));

        protected void WaitId()
        {
            while (!(BenchmarkContext.TryGet(out var id) && _currentId == id)) ;
        }
    }
}
