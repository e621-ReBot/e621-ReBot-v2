using System;
using System.Security.Cryptography;
using System.Text;

namespace e621_ReBot_v2.Modules
{
    public class Module_Cryptor
    {
        public static string Encrypt(string PlainText)
        {
            AesCryptoServiceProvider AES = new AesCryptoServiceProvider()
            {
                Key = Encoding.ASCII.GetBytes("e621-126621-126e"),
                IV = new byte[] { 0, 1, 2, 3, 4, 6, 8, 12, 16, 20, 24, 32, 48, 64, 80, 96 },
                Mode = CipherMode.CBC
            };
            ICryptoTransform Encrypter = AES.CreateEncryptor();
            byte[] PlainTextBytes = Encoding.UTF8.GetBytes(PlainText);
            string EncryptedText = Convert.ToBase64String(Encrypter.TransformFinalBlock(PlainTextBytes, 0, PlainTextBytes.Length));
            AES.Dispose();
            return EncryptedText;
        }

        public static string Decrypt(string EncryptedText)
        {
            AesCryptoServiceProvider AES = new AesCryptoServiceProvider()
            {
                Key = Encoding.ASCII.GetBytes("e621-126621-126e"),
                IV = new byte[] { 0, 1, 2, 3, 4, 6, 8, 12, 16, 20, 24, 32, 48, 64, 80, 96 },
                Mode = CipherMode.CBC
            };
            ICryptoTransform DESDecrypter = AES.CreateDecryptor();
            byte[] EncryptedTextBytes = Convert.FromBase64String(EncryptedText);
            string DecryptedText = Encoding.ASCII.GetString(DESDecrypter.TransformFinalBlock(EncryptedTextBytes, 0, EncryptedTextBytes.Length));
            AES.Dispose();
            return DecryptedText;
        }

    }
}
