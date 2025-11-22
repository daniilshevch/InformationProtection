using InformationProtection1.Services.Lab5.Implementations;
using System.IO;
using System.Text;
using Xunit;

namespace SecurityLab5.Tests
{
    public class DsaServiceTests
    {
        private readonly DsaService _service;

        public DsaServiceTests()
        {
            _service = new DsaService();
        }

        [Fact]
        public void GenerateKeys_ShouldReturnValidPemKeys()
        {
            var keys = _service.GenerateKeys();

            Assert.NotNull(keys);
            Assert.False(string.IsNullOrEmpty(keys.PrivateKey));
            Assert.False(string.IsNullOrEmpty(keys.PublicKey));

            Assert.Contains("BEGIN PRIVATE KEY", keys.PrivateKey);
            Assert.Contains("BEGIN PUBLIC KEY", keys.PublicKey);
        }

        [Fact]
        public void SignAndVerifyData_ShouldWorkForValidSignature()
        {
            var keys = _service.GenerateKeys();
            string data = "Test Message for Signature";
            string signatureHex = _service.SignData(data, keys.PrivateKey);
            bool isValid = _service.VerifyData(data, signatureHex, keys.PublicKey);
            Assert.True(isValid, "Підпис має бути валідним для тих самих даних і ключів");
        }

        [Fact]
        public void VerifyData_ShouldFail_WhenDataIsTampered()
        {
            var keys = _service.GenerateKeys();
            string originalData = "Original Message";
            string tamperedData = "Modified Message";
            string signatureHex = _service.SignData(originalData, keys.PrivateKey);
            bool isValid = _service.VerifyData(tamperedData, signatureHex, keys.PublicKey);
            Assert.False(isValid, "Перевірка має провалитися, якщо дані змінено");
        }

        [Fact]
        public void VerifyData_ShouldFail_WhenWrongPublicKeyUsed()
        {
            var keysA = _service.GenerateKeys();
            var keysB = _service.GenerateKeys();
            string data = "Secret Data";
            string signatureHex = _service.SignData(data, keysA.PrivateKey);
            bool isValid = _service.VerifyData(data, signatureHex, keysB.PublicKey);
            Assert.False(isValid, "Перевірка має провалитися з чужим публічним ключем");
        }

        [Fact]
        public void SignAndVerifyFile_ShouldWork()
        {
            var keys = _service.GenerateKeys();
            string fileContent = "This is file content";
            byte[] fileBytes = Encoding.UTF8.GetBytes(fileContent);
            string signatureHex;
            using (var stream = new MemoryStream(fileBytes))
            {
                signatureHex = _service.SignFile(stream, keys.PrivateKey);
            }
            bool isValid;
            using (var stream = new MemoryStream(fileBytes))
            {
                isValid = _service.VerifyFile(stream, signatureHex, keys.PublicKey);
            }
            Assert.True(isValid);
        }
    }
}