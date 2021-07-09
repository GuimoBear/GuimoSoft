using System.Threading.Tasks;

namespace GuimoSoft.Cache.Delegates
{
    public delegate Task<TValue> AsyncValueFactory<TValue>();
}
