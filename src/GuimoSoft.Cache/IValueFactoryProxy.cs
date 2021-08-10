using System.Threading.Tasks;
using GuimoSoft.Cache.Delegates;

namespace GuimoSoft.Cache
{
    public interface IValueFactoryProxy<TValue>
    {
        TValue Produce(ValueFactory<TValue> valueFactory);
        Task<TValue> ProduceAsync(AsyncValueFactory<TValue> asyncValueFactory);
    }
}
