using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace GuimoSoft.Benchmark.Bus.Handlers.Benchmark
{
    public class BenchmarkNotificationHandler : INotificationHandler<BenchmarkEvent>
    {
        public Task Handle(BenchmarkEvent notification, CancellationToken cancellationToken)
        {
            BenchmarkContext.Add(notification.Id);
            return Task.CompletedTask;
        }
    }
}
