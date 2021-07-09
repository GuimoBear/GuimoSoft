using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GuimoSoft.Cache.Utils;

namespace GuimoSoft.Cache.InMemory
{
    internal sealed partial class TypedCache<TKey, TValue>
    {
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly Task _clearInstances;

        private async Task ClearInstances()
        {
            await DelayToNextCleaning();
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                ClearCache();
                await DelayToNextCleaning();
            }
        }

        private void ClearCache()
        {
            try
            {
                using var locker = new DisposableLock(_lock);
                var now = DateTime.UtcNow;
                var valuesToExclude = _cache.Where(item => item.Value.TTL < now).ToList();
                foreach (var kvp in valuesToExclude)
                    _cache.Remove(kvp);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Houve um erro ao limpar o cache: {ex.Message}");
            }
        }

        private async Task DelayToNextCleaning()
        {
            var nextExecutionTime = DateTime.UtcNow.Add(_configs.CleaningInterval);
            while (!_cancellationTokenSource.IsCancellationRequested && nextExecutionTime > DateTime.UtcNow)
                await Task.Delay(_configs.DelayToNextCancellationRequestedCheck).ConfigureAwait(false);
        }
    }
}
