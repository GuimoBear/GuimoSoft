using System;
using System.Runtime.Serialization;

namespace GuimoSoft.Cryptography.RSA.Exceptions
{
    [Serializable]
    public sealed class RsaIdentifierNotInformedException : Exception
    {
        public RsaIdentifierNotInformedException()
            : base($"É necessário que seja informado um identificador existente da chave RSA") { }

        private RsaIdentifierNotInformedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
