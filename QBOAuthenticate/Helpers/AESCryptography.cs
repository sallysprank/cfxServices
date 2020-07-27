using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace QBOAuthenticate.Helpers
{
    public class AESCryptography
    {
        protected IConfiguration _configuration;
        public AESCryptography(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string Encrypt(string plainText)
        {
            byte[] Key = Encoding.ASCII.GetBytes(_configuration["AESHelper:EncryptionKey"]);
            byte[] IV = Encoding.ASCII.GetBytes(_configuration["AESHelper:InitialisationVector"]);
            byte[] encrypted;
            // Create a new AesManaged.    
            using (AesManaged aes = new AesManaged())
            {
                // Create encryptor    
                ICryptoTransform encryptor = aes.CreateEncryptor(Key, IV);
                // Create MemoryStream    
                using (MemoryStream ms = new MemoryStream())
                {
                    // Create crypto stream using the CryptoStream class. This class is the key to encryption    
                    // and encrypts and decrypts data from any given stream. In this case, we will pass a memory stream    
                    // to encrypt    
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        // Create StreamWriter and write data to a stream    
                        using (StreamWriter sw = new StreamWriter(cs))
                            sw.Write(plainText);
                        encrypted = ms.ToArray();
                    }
                }
            }
            // Return encrypted data    
            return Convert.ToBase64String(encrypted);
        }
        public string Decrypt(string cipherText)
        {
            byte[] Key = Encoding.ASCII.GetBytes(_configuration["AESHelper:EncryptionKey"]);
            byte[] IV = Encoding.ASCII.GetBytes(_configuration["AESHelper:InitialisationVector"]);
            byte[] cipherTextByte = Convert.FromBase64String(cipherText);
            string plaintext = null;
            // Create AesManaged    
            using (AesManaged aes = new AesManaged())
            {
                // Create a decryptor    
                ICryptoTransform decryptor = aes.CreateDecryptor(Key, IV);
                // Create the streams used for decryption.    
                using (MemoryStream ms = new MemoryStream(cipherTextByte))
                {
                    // Create crypto stream    
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        // Read crypto stream    
                        using (StreamReader reader = new StreamReader(cs))
                            plaintext = reader.ReadToEnd();
                    }
                }
            }
            return plaintext;
        }
    }
}
