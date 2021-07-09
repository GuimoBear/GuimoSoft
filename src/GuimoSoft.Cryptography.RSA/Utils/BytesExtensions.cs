namespace GuimoSoft.Cryptography.RSA.Utils
{
    internal static class BytesExtensions
    {
        internal static ushort CRC16(this byte[] source, ushort crc = 0)
        {
            return Utils.Crc16.Compute(source, crc);
        }

        internal static byte[] XOR(this byte[] source, byte[] password)
        {
            var ret = new byte[source.Length];
            for (int index = 0; index < source.Length; index++)
            {
                ret[index] = (byte)(source[index] ^ password[index % password.Length]);
            }
            return ret;
        }
    }
}
