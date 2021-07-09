using GuimoSoft.Cache.Delegates;
using System.Threading.Tasks;

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
