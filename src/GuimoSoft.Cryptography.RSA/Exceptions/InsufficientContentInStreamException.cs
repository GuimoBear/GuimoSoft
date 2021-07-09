using System;
using System.Runtime.Serialization;

namespace GuimoSoft.Cryptography.RSA.Exceptions
{
    [Serializable]
    public sealed class InsufficientContentInStreamException : Exception
    {
        public InsufficientContentInStreamException(string packageName)
            : base($"A stream não tem conteúdo suficiente para ser deserializada no pacote {packageName}") { }

        public InsufficientContentInStreamException() : base() { }
        private InsufficientContentInStreamException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
