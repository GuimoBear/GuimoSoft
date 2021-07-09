using System;
using System.Runtime.Serialization;

namespace GuimoSoft.Cryptography.RSA.Exceptions
{
    [Serializable]
    public sealed class CorruptedPackageException : Exception
    {
        public readonly string PackageName;

        public CorruptedPackageException(string packageName)
            : base($"O pacote {packageName} está corrompido e não deve ser usado")
        {
            PackageName = packageName;
        }

        public CorruptedPackageException() : base() { }
        private CorruptedPackageException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
