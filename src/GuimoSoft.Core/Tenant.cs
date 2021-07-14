using System;

namespace GuimoSoft.Core
{
    public sealed class Tenant : IEquatable<Tenant>, IEquatable<string>
    {
        public string Value { get; }

        public Tenant(string value)
        {
            Value = value ?? string.Empty;
        }

        public override bool Equals(object obj)
        {
            if (obj is Tenant tenant)
                return Equals(tenant);
            if (obj is string str)
                return Equals(str);
            return false;
        }

        public bool Equals(string other)
        {
            return Value.Equals(other ?? "");
        }

        public bool Equals(Tenant other)
        {
            return other?.Value?.Equals(Value) ?? false;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value;
        }

        public static implicit operator Tenant(string origem)
        {
            return new Tenant(origem);
        }

        public static implicit operator string(Tenant origem)
        {
            return origem?.Value ?? "";
        }
    }
}
