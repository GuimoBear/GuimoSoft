using GuimoSoft.Cryptography.RSA.Exceptions;
using GuimoSoft.Cryptography.RSA.Utils;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace GuimoSoft.Cryptography.RSA.Packets
{
    public class PasswordEncryptedPackage
    {
        private const string _SALT = "_0f9ewf*675";
        private const string SALT_ = "^`)9fsd7f&f";
        public const int CRC_SIZE = 2;
        public const int PACKAGE_LENGTH_SIZE = 4;
        public const int MINIMUM_PASSWORD_AFTER_TRIM_LENGTH = 8;

        private readonly byte[] body;

        public PasswordEncryptedPackage(byte[] body)
        {
            if (body == default(byte[]))
                throw new ArgumentNullException(nameof(body), "O " + nameof(body) + " não deve ser nulo");
            this.body = body;
        }

        public byte[] Encrypt(string password, Encoding encoding = default)
        {
            ValidatePassword(password);
            password = password.Trim();
            encoding = GetEncodingOrDefault(encoding);

            using (var ms = new MemoryStream())
            {
                var crc = body.CRC16();
                ms.Write(BitConverter.GetBytes(Convert.ToUInt32(body.Length)), 0, PACKAGE_LENGTH_SIZE);
                ms.Write(body.XOR(encoding.GetBytes(SALT_ + password + _SALT)), 0, body.Length);
                ms.Write(BitConverter.GetBytes(crc), 0, CRC_SIZE);

                return ms.ToArray();
            }
        }
        public async Task<byte[]> EncryptAsync(string password, Encoding encoding = default)
        {
            ValidatePassword(password);
            password = password.Trim();
            encoding = GetEncodingOrDefault(encoding);

            using (var ms = new MemoryStream())
            {
                var crc = body.CRC16();

                await ms.WriteAsync(BitConverter.GetBytes(Convert.ToUInt32(body.Length)).AsMemory(0, PACKAGE_LENGTH_SIZE));
                await ms.WriteAsync(body.XOR(encoding.GetBytes(SALT_ + password + _SALT)).AsMemory(0, body.Length));
                await ms.WriteAsync(BitConverter.GetBytes(crc).AsMemory(0, CRC_SIZE));

                return ms.ToArray();
            }
        }

        public byte[] Decrypt(string password, Encoding encoding = default)
        {
            ValidatePassword(password);
            password = password.Trim();
            encoding = GetEncodingOrDefault(encoding);

            using (var ms = new MemoryStream(body))
            {
                ms.Position = 0;
                var passwordBytes = encoding.GetBytes(SALT_ + password + _SALT);

                return ReadFromStream(ms, passwordBytes);
            }
        }
        public async Task<byte[]> DecryptAsync(string password, Encoding encoding = default)
        {
            ValidatePassword(password);
            password = password.Trim();
            encoding = GetEncodingOrDefault(encoding);

            using (var ms = new MemoryStream(body))
            {
                ms.Position = 0;
                var passwordBytes = encoding.GetBytes(SALT_ + password + _SALT);

                return await ReadFromStreamAsync(ms, passwordBytes);
            }
        }

        private static byte[] ReadFromStream(Stream stream, byte[] passwordBytes)
        {
            var lengthBuffer = new byte[PACKAGE_LENGTH_SIZE];
            stream.Read(lengthBuffer, 0, PACKAGE_LENGTH_SIZE);

            var buffer = new byte[BitConverter.ToUInt32(lengthBuffer, 0)];
            stream.Read(buffer, 0, buffer.Length);

            buffer = buffer.XOR(passwordBytes);

            var calculatedCrc = buffer.CRC16();

            var crcBuffer = new byte[CRC_SIZE];
            stream.Read(crcBuffer, 0, CRC_SIZE);
            var receivedCrc = BitConverter.ToUInt16(crcBuffer, 0);

            if (calculatedCrc != receivedCrc)
                throw new CorruptedPackageException(nameof(PasswordEncryptedPackage));

            return buffer;
        }
        private static async Task<byte[]> ReadFromStreamAsync(Stream stream, byte[] passwordBytes)
        {
            var lengthBuffer = new byte[PACKAGE_LENGTH_SIZE];
            await stream.ReadAsync(lengthBuffer.AsMemory(0, PACKAGE_LENGTH_SIZE));

            var buffer = new byte[BitConverter.ToUInt32(lengthBuffer, 0)];
            await stream.ReadAsync(buffer.AsMemory(0, buffer.Length));

            buffer = buffer.XOR(passwordBytes);

            var calculatedCrc = buffer.CRC16();

            var crcBuffer = new byte[CRC_SIZE];
            await stream.ReadAsync(crcBuffer.AsMemory(0, CRC_SIZE));
            var receivedCrc = BitConverter.ToUInt16(crcBuffer, 0);

            if (calculatedCrc != receivedCrc)
                throw new CorruptedPackageException(nameof(PasswordEncryptedPackage));

            return buffer;
        }

        private static void ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Trim().Length < MINIMUM_PASSWORD_AFTER_TRIM_LENGTH)
                throw new ArgumentException($"O {nameof(password)} Deve conter ao menos {MINIMUM_PASSWORD_AFTER_TRIM_LENGTH} caracteres após a remoção dos espaços em branco", nameof(password));
        }

        private static Encoding GetEncodingOrDefault(Encoding encoding)
        {
            if (encoding != default(Encoding))
                return encoding;
            return Encoding.UTF8;
        }
    }
}
