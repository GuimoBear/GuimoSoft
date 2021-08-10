using System.Threading.Tasks;
using GuimoSoft.Cache.Delegates;

namespace GuimoSoft.Cache.Utils
{
    internal class DefaultValueFactoryProxy<TValue> : IValueFactoryProxy<TValue>
    {
        public static readonly IValueFactoryProxy<TValue> Instance
            = new DefaultValueFactoryProxy<TValue>();

        private DefaultValueFactoryProxy() { }

        public TValue Produce(ValueFactory<TValue> valueFactory)
            => valueFactory();

        public Task<TValue> ProduceAsync(AsyncValueFactory<TValue> asyncValueFactory)
            => asyncValueFactory();
    }
}
