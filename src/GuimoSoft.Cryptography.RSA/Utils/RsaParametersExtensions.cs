using System.Security.Cryptography;

namespace GuimoSoft.Cryptography.RSA.Utils
{
    internal static class RsaParametersExtensions
    {
        internal static bool CanEncrypt(this RSAParameters rsaParameters)
        {
            return
                rsaParameters.Exponent != default(byte[]) && rsaParameters.Exponent.Length > 0 &&
                rsaParameters.Modulus != default(byte[]) && rsaParameters.Modulus.Length > 0;
        }

        internal static bool CanDecrypt(this RSAParameters rsaParameters)
        {
            return
                rsaParameters.D != default(byte[]) && rsaParameters.D.Length > 0 &&
                rsaParameters.DP != default(byte[]) && rsaParameters.DP.Length > 0 &&
                rsaParameters.DQ != default(byte[]) && rsaParameters.DQ.Length > 0 &&
                rsaParameters.InverseQ != default(byte[]) && rsaParameters.InverseQ.Length > 0 &&
                rsaParameters.P != default(byte[]) && rsaParameters.P.Length > 0 &&
                rsaParameters.Q != default(byte[]) && rsaParameters.Q.Length > 0;
        }
    }
}
