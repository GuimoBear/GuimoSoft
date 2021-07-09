using System;
using System.Runtime.Serialization;

namespace GuimoSoft.Cache.Exceptions
{
    [Serializable]
    public sealed class CacheNotConfiguredException : Exception
    {
        public CacheNotConfiguredException(Type keyType, Type valueType)
            : base($"Não existe um cache configurado com a chave do tipo {keyType.Name} e o valor do tipo {valueType.Name}")
        {

        }

        public CacheNotConfiguredException() : base() { }
        private CacheNotConfiguredException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
