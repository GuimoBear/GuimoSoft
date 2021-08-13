using System;

namespace GuimoSoft.Core
{
    public sealed class CorrelationId : IEquatable<CorrelationId>, IEquatable<string>
    {
        public string Value { get; }

        private CorrelationId(string correlationId)
        {
            Value = correlationId;
        }

        public override bool Equals(object obj)
        {
            if (obj is CorrelationId correlationId)
                return Equals(correlationId);
            if (obj is string strCorrelationId)
                return Equals(strCorrelationId);
            return false;
        }

        public bool Equals(string other)
        {
            if (other is null)
                return false;
            return Value.Equals(other);
        }

        public bool Equals(CorrelationId other)
        {
            if (other is null)
                return false;
            return other.Value.Equals(Value);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value;
        }

        public static implicit operator CorrelationId(string correlationId)
        {
            return string.IsNullOrEmpty(correlationId) ?
                new CorrelationId(Guid.NewGuid().ToString()) :
                new CorrelationId(correlationId);
        }

        public static implicit operator string(CorrelationId correlationId)
        {
            return correlationId?.Value;
        }
    }
}
