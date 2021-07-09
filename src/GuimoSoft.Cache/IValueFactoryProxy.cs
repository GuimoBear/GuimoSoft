using GuimoSoft.Cache.Delegates;
using System.Threading.Tasks;

namespace GuimoSoft.Cache
{
    public interface IValueFactoryProxy<TValue>
    {
        TValue Produce(ValueFactory<TValue> valueFactory);
        Task<TValue> ProduceAsync(AsyncValueFactory<TValue> asyncValueFactory);
    }
}
