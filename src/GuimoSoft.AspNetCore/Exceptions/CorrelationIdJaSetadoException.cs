using System;
using System.Runtime.Serialization;

namespace GuimoSoft.AspNetCore.Exceptions
{
    [Serializable]
    public sealed class CorrelationIdJaSetadoException : Exception
    {
        public CorrelationIdJaSetadoException(CorrelationId atual, CorrelationId novo)
            : base($"O CorrelationId já foi setado anteriormente(Atual: {atual}, Novo valor: {novo})") { }

        public CorrelationIdJaSetadoException() : base() { }
        private CorrelationIdJaSetadoException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
