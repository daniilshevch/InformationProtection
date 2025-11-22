using InformationProtection1.Services.Lab4.Interfaces;
using System.Diagnostics;
using System.Security.Cryptography;

namespace InformationProtection1.Services.Lab4.Implementations
{
    public class RSAService : IRSAService
    {
        private const int AesKeySize = 32;
        private const int AesIvSize = 16;

        public (string publicKeyPem, string privateKeyPem) GenerateKeys()
        {
            using var rsa = RSA.Create(2048);

            string publicKeyPem = rsa.ExportRSAPublicKeyPem();
            string privateKeyPem = rsa.ExportRSAPrivateKeyPem();

            return (publicKeyPem, privateKeyPem);
        }

        public async Task<long> EncryptStreamAsync(Stream inputStream, string publicKeyPem, Stream outputStream)
        {
            Stopwatch sw = Stopwatch.StartNew();
            using var rsa = RSA.Create();
            rsa.ImportFromPem(publicKeyPem);

            using var aes = Aes.Create();
            aes.KeySize = AesKeySize * 8;
            aes.BlockSize = AesIvSize * 8;
            aes.Padding = PaddingMode.PKCS7;
            aes.GenerateKey();
            aes.GenerateIV();

            byte[] aesKey = aes.Key;
            byte[] aesIV = aes.IV;

            byte[] keyAndIv = new byte[AesKeySize + AesIvSize];
            Buffer.BlockCopy(aesKey, 0, keyAndIv, 0, AesKeySize);
            Buffer.BlockCopy(aesIV, 0, keyAndIv, AesKeySize, AesIvSize);

            byte[] encryptedAesKeyAndIv = rsa.Encrypt(keyAndIv, RSAEncryptionPadding.OaepSHA256);

            await outputStream.WriteAsync(BitConverter.GetBytes(encryptedAesKeyAndIv.Length), 0, 4);

            await outputStream.WriteAsync(encryptedAesKeyAndIv, 0, encryptedAesKeyAndIv.Length);

            using (var cryptoStream = new CryptoStream(outputStream, aes.CreateEncryptor(), CryptoStreamMode.Write, true))
            {
                await inputStream.CopyToAsync(cryptoStream);
            }
            sw.Stop();
            return sw.ElapsedMilliseconds;
        }

        public async Task<long> DecryptStreamAsync(Stream inputStream, string privateKeyPem, Stream outputStream)
        {
            Stopwatch sw = Stopwatch.StartNew();
            using var rsa = RSA.Create();
            rsa.ImportFromPem(privateKeyPem);

            byte[] lengthBuffer = new byte[4];
            await ReadExactlyAsync(inputStream, lengthBuffer, 0, 4);
            int encryptedKeyLength = BitConverter.ToInt32(lengthBuffer, 0);

            byte[] encryptedAesKeyAndIv = new byte[encryptedKeyLength];
            await ReadExactlyAsync(inputStream, encryptedAesKeyAndIv, 0, encryptedKeyLength);

            byte[] decryptedKeyAndIv = rsa.Decrypt(encryptedAesKeyAndIv, RSAEncryptionPadding.OaepSHA256);

            byte[] aesKey = new byte[AesKeySize];
            byte[] aesIV = new byte[AesIvSize];
            Buffer.BlockCopy(decryptedKeyAndIv, 0, aesKey, 0, AesKeySize);
            Buffer.BlockCopy(decryptedKeyAndIv, AesKeySize, aesIV, 0, AesIvSize);

            using var aes = Aes.Create();
            aes.KeySize = AesKeySize * 8;
            aes.BlockSize = AesIvSize * 8;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = aesKey;
            aes.IV = aesIV;

            using (var cryptoStream = new CryptoStream(inputStream, aes.CreateDecryptor(), CryptoStreamMode.Read, true))
            {
                await cryptoStream.CopyToAsync(outputStream);
            }
            sw.Stop();
            return sw.ElapsedMilliseconds;
        }

        private async Task ReadExactlyAsync(Stream stream, byte[] buffer, int offset, int count)
        {
            int bytesRead = 0;
            while (bytesRead < count)
            {
                int read = await stream.ReadAsync(buffer, offset + bytesRead, count - bytesRead);
                if (read == 0)
                    throw new EndOfStreamException("Неможливо прочитати дані, потік завершився.");
                bytesRead += read;
            }
        }
    }
}
