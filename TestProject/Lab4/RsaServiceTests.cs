using InformationProtection1.Services.Lab4.Implementations;
using System.Security.Cryptography;
using System.Text;

namespace InformationProtection1.Tests.Lab4.Services
{
    public class RSAServiceTests
    {
        private readonly RSAService service = new();

        [Fact]
        public void GenerateKeys_ShouldReturnValidPemStrings()
        {
            var (publicKey, privateKey) = service.GenerateKeys();

            Assert.False(string.IsNullOrWhiteSpace(publicKey));
            Assert.False(string.IsNullOrWhiteSpace(privateKey));

            Assert.Contains("BEGIN RSA PUBLIC KEY", publicKey); 
            Assert.Contains("BEGIN RSA PRIVATE KEY", privateKey); 

            using var rsa = RSA.Create();
            rsa.ImportFromPem(publicKey);
            rsa.ImportFromPem(privateKey);
        }

        [Fact]
        public async Task EncryptDecryptStream_ShouldReturnOriginalData()
        {
            string originalText = "This is a secret message for RSA lab test.";
            byte[] originalBytes = Encoding.UTF8.GetBytes(originalText);

            var (publicKey, privateKey) = service.GenerateKeys();

            using var inputStream = new MemoryStream(originalBytes);
            using var encryptedStream = new MemoryStream();
            using var decryptedStream = new MemoryStream();

            long encTime = await service.EncryptStreamAsync(inputStream, publicKey, encryptedStream);

            Assert.True(encTime >= 0);
            Assert.True(encryptedStream.Length > 0);
            Assert.NotEqual(originalBytes.Length, encryptedStream.Length); 

            encryptedStream.Position = 0; 
            long decTime = await service.DecryptStreamAsync(encryptedStream, privateKey, decryptedStream);

            decryptedStream.Position = 0;
            string decryptedText = Encoding.UTF8.GetString(decryptedStream.ToArray());

            Assert.True(decTime >= 0);
            Assert.Equal(originalText, decryptedText);
        }

        [Fact]
        public async Task DecryptStream_ShouldThrow_WithWrongKey()
        {
            byte[] data = Encoding.UTF8.GetBytes("test");
            var (pubKey1, _) = service.GenerateKeys();
            var (_, privKey2) = service.GenerateKeys(); 

            using var inputStream = new MemoryStream(data);
            using var encryptedStream = new MemoryStream();

            await service.EncryptStreamAsync(inputStream, pubKey1, encryptedStream);

            encryptedStream.Position = 0;
            using var decryptedStream = new MemoryStream();

            await Assert.ThrowsAnyAsync<CryptographicException>(() =>
                service.DecryptStreamAsync(encryptedStream, privKey2, decryptedStream));
        }

        [Fact]
        public async Task DecryptStream_ShouldThrow_WithCorruptedData()
        {
            var (_, privKey) = service.GenerateKeys();
            byte[] corruptedData = new byte[] { 1, 2, 3, 4, 5 }; 

            using var inputStream = new MemoryStream(corruptedData);
            using var decryptedStream = new MemoryStream();

            await Assert.ThrowsAnyAsync<Exception>(() =>
                service.DecryptStreamAsync(inputStream, privKey, decryptedStream));
        }
    }
}