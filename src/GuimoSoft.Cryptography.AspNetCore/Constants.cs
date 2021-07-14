namespace GuimoSoft.Cryptography.AspNetCore
{
    public static class Constants
    {
        public const string ENCRYPTED_CONTENT_TYPE = "application/encrypted-json";
        public const string ENCRYPTED_BASE64_CONTENT_TYPE = "application/base64-encrypted-json";
        public const string RSA_IDENTIFIER_HEADER = "x-cert-id";
        internal const string CERTIFICATE_FILE_EXTENSION = "crt";
    }
}
