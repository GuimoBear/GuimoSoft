using GuimoSoft.Bus.Abstractions.Consumer;
using System.Threading;
using System.Threading.Tasks;

namespace GuimoSoft.Benchmark.Bus.Handlers.Benchmark
{
    public class BenchmarkEventHandler : IEventHandler<BenchmarkEvent>
    {
        public Task Handle(BenchmarkEvent @event, CancellationToken cancellationToken)
        {
            BenchmarkContext.Add(@event.Id);
            return Task.CompletedTask;
        }
    }
}
