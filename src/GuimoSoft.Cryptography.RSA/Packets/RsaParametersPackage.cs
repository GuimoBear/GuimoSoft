using GuimoSoft.Cryptography.RSA.Exceptions;
using GuimoSoft.Cryptography.RSA.Utils;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace GuimoSoft.Cryptography.RSA.Packets
{
    public class RsaParametersPackage : PackageBase
    {
        public const int GUID_LENGTH_SIZE = 16;
        public const int PART_LENGTH_SIZE = 2;
        public const int INCLUDE_PRIVATE_PARAMETERS_SIZE = 1;
        public const int CRC_SIZE = 2;

        public Guid Identifier { get; }
        public RSAParameters Content { get; }
        public override byte[] Bytes { get; }

        public RsaParametersPackage(Guid identifier, RSAParameters content)
        {
            Identifier = identifier;
            Content = content;
            Bytes = GetBytes();
        }

        public RsaParametersPackage(byte[] content)
        {
            if (content == default(byte[]))
                throw new ArgumentNullException(nameof(content), $"O {nameof(content)} não deve ser nulo");
            Bytes = content;
            Content = FromBytes(out var identifier);
            Identifier = identifier;
        }

        public async Task WriteIntoStreamAsync(Stream stream)
        {
            await stream.WriteAsync(Bytes.AsMemory(0, Bytes.Length));
        }

        private byte[] GetBytes()
        {
            using (var ms = new MemoryStream())
            {
                int partLength = Content.Modulus.Length / 2;
                var includePrivateParameters = IncludePrivateParameters(Content, partLength);

                var identifierBytes = Identifier.ToByteArray();
                var crc = Utils.Crc16.Compute(identifierBytes);
                ms.Write(identifierBytes, 0, GUID_LENGTH_SIZE);
                ms.Write(BitConverter.GetBytes(Convert.ToUInt16(partLength)), 0, 2);
                ms.Write(BitConverter.GetBytes(includePrivateParameters), 0, 1);

                crc = ComputeCRCAndWriteIntoStream(Content.Modulus, ms, crc);
                crc = ComputeCRCAndWriteIntoStream(Content.Exponent, ms, crc);
                if (includePrivateParameters)
                {
                    crc = ComputeCRCAndWriteIntoStream(Content.D, ms, crc);
                    crc = ComputeCRCAndWriteIntoStream(Content.DP, ms, crc);
                    crc = ComputeCRCAndWriteIntoStream(Content.DQ, ms, crc);
                    crc = ComputeCRCAndWriteIntoStream(Content.InverseQ, ms, crc);
                    crc = ComputeCRCAndWriteIntoStream(Content.P, ms, crc);
                    crc = ComputeCRCAndWriteIntoStream(Content.Q, ms, crc);
                }
                ms.Write(BitConverter.GetBytes(crc), 0, 2);
                return ms.ToArray();
            }
        }

        private RSAParameters FromBytes(out Guid identifier)
        {
            using (var ms = new MemoryStream(Bytes))
            {
                ms.Position = 0;
                identifier = ReadIdentifierFromStream(ms, out var crc);
                return ReadParametersFromStream(ms, crc);
            }
        }

        private static Guid ReadIdentifierFromStream(Stream stream, out ushort crc)
        {
            var buffer = new byte[GUID_LENGTH_SIZE];
            var count = stream.Read(buffer, 0, GUID_LENGTH_SIZE);
            if (count != GUID_LENGTH_SIZE)
                throw new InsufficientContentInStreamException(nameof(RsaParametersPackage));
            crc = Utils.Crc16.Compute(buffer);
            return new Guid(buffer);
        }

        private static RSAParameters ReadParametersFromStream(Stream stream, ushort crc)
        {
            var partLength = GetPartLength(stream);
            var includePrivateParameters = GetIfIncludePrivateParameters(stream);
            var parameters = CreateRSAParametersInstance(partLength, includePrivateParameters);
            crc = ComputeCRCAndReadStream(stream, parameters.Modulus, crc);
            crc = ComputeCRCAndReadStream(stream, parameters.Exponent, crc);
            if (includePrivateParameters)
            {
                crc = ComputeCRCAndReadStream(stream, parameters.D, crc);
                crc = ComputeCRCAndReadStream(stream, parameters.DP, crc);
                crc = ComputeCRCAndReadStream(stream, parameters.DQ, crc);
                crc = ComputeCRCAndReadStream(stream, parameters.InverseQ, crc);
                crc = ComputeCRCAndReadStream(stream, parameters.P, crc);
                crc = ComputeCRCAndReadStream(stream, parameters.Q, crc);
            }
            var packageCrc = GetCRC(stream);
            if (crc != packageCrc)
                throw new CorruptedPackageException(nameof(RsaParametersPackage));
            return parameters;
        }

        private static RSAParameters CreateRSAParametersInstance(ushort partLength, bool includePrivateParameters)
        {
            var parameters = new RSAParameters
            {
                Modulus = new byte[partLength * 2],
                Exponent = new byte[3]
            };

            if (includePrivateParameters)
            {
                parameters.D = new byte[partLength * 2];
                parameters.DP = new byte[partLength];
                parameters.DQ = new byte[partLength];
                parameters.InverseQ = new byte[partLength];
                parameters.P = new byte[partLength];
                parameters.Q = new byte[partLength];
            }

            return parameters;
        }

        private static ushort ComputeCRCAndWriteIntoStream(byte[] bytes, Stream destination, ushort crc = 0)
        {
            destination.Write(bytes, 0, bytes.Length);
            return bytes.CRC16(crc);
        }

        private static ushort ComputeCRCAndReadStream(Stream source, byte[] destination, ushort crc = 0)
        {
            var count = source.Read(destination, 0, destination.Length);
            if (count != destination.Length)
                throw new InsufficientContentInStreamException(nameof(RsaParametersPackage));
            return destination.CRC16(crc);
        }

        private static ushort GetPartLength(Stream origin)
        {
            var buffer = new byte[2];
            var count = origin.Read(buffer, 0, 2);
            if (count != 2)
                throw new InsufficientContentInStreamException(nameof(RsaParametersPackage));
            return BitConverter.ToUInt16(buffer, 0);
        }

        private static bool GetIfIncludePrivateParameters(Stream origin)
        {
            var @byte = origin.ReadByte();
            if (@byte == -1)
                throw new InsufficientContentInStreamException(nameof(RsaParametersPackage));
            return Convert.ToBoolean(@byte);
        }

        private static ushort GetCRC(Stream origin)
        {
            var buffer = new byte[2];
            var count = origin.Read(buffer, 0, CRC_SIZE);
            if (count != CRC_SIZE)
                throw new InsufficientContentInStreamException(nameof(RsaParametersPackage));
            return BitConverter.ToUInt16(buffer, 0);
        }

        private static bool IncludePrivateParameters(RSAParameters rsaParameters, int partLength)
        {
            return
                rsaParameters.D != default(byte[]) && rsaParameters.D.Length == partLength * 2 &&
                rsaParameters.DP != default(byte[]) && rsaParameters.DP.Length == partLength &&
                rsaParameters.DQ != default(byte[]) && rsaParameters.DQ.Length == partLength &&
                rsaParameters.InverseQ != default(byte[]) && rsaParameters.InverseQ.Length == partLength &&
                rsaParameters.P != default(byte[]) && rsaParameters.P.Length == partLength &&
                rsaParameters.Q != default(byte[]) && rsaParameters.Q.Length == partLength;
        }
    }
}
