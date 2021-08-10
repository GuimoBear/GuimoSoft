using Microsoft.Extensions.Logging;
using System;

namespace GuimoSoft.Logger.Utils
{
    internal struct TypeLogLevelKey
    {
        private readonly int hashCode;

        public TypeLogLevelKey(Type type, LogLevel logLevel)
        {
            hashCode = HashCode.Combine(type, logLevel);
        }

        public override int GetHashCode()
            => hashCode;

        public override bool Equals(object obj)
            => obj is TypeLogLevelKey key &&
               hashCode.Equals(key.hashCode);
    }
}
