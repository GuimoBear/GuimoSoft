using GuimoSoft.Cache.Utils;

namespace GuimoSoft.Cache
{
    public sealed class ValueFactory
    {
        public static readonly ValueFactory Instance
            = new ValueFactory();

        private ValueFactory() { }
    }

    public static class ValueFactoryExtensions
    {
        public static IValueFactoryProxy<TValue> Default<TValue>(this ValueFactory _)
            => DefaultValueFactoryProxy<TValue>.Instance;
    }
}
