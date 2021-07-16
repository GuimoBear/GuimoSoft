using GuimoSoft.Cache.Delegates;
using Polly;
using System;
using System.Threading.Tasks;

namespace GuimoSoft.Cache.Resilience
{
    internal class ResilientValueFactoryProxy<TValue> : IValueFactoryProxy<TValue>
    {
        private readonly IAsyncPolicy<TValue> _asyncPolicy;

        public ResilientValueFactoryProxy(IAsyncPolicy<TValue> asyncPolicy)
        {
            _asyncPolicy = asyncPolicy ?? throw new ArgumentNullException(nameof(asyncPolicy));
        }

        public TValue Produce(ValueFactory<TValue> valueFactory)
        {
            return ProduceAsync(() => Task.FromResult(valueFactory()))
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }

        public async Task<TValue> ProduceAsync(AsyncValueFactory<TValue> asyncValueFactory)
            => await _asyncPolicy.ExecuteAsync(() => asyncValueFactory());
    }
}
