using System;
using System.Runtime.Serialization;

namespace GuimoSoft.AspNetCore.Exceptions
{
    [Serializable]
    public sealed class TenantJaSetadoException : Exception
    {
        public TenantJaSetadoException(Tenant atual, Tenant novo)
            : base($"O Tenant já foi setado anteriormente(Atual: {atual}, Novo valor: {novo})") { }

        public TenantJaSetadoException() : base() { }
        private TenantJaSetadoException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
