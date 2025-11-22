using InformationProtection1.Controllers.Lab3;
using InformationProtection1.Dto.Lab3;
using InformationProtection1.Services.Lab3.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace InformationProtection1.Tests.Lab3.Controllers
{
    public class Rc5EncryptionControllerTests
    {
        private readonly Mock<IRC5EncryptionService> mockService = new();
        private readonly Rc5EncryptionController controller;

        public Rc5EncryptionControllerTests()
        {
            controller = new Rc5EncryptionController(mockService.Object);
        }

        [Fact]
        public void EncryptText_ShouldReturnOkWithResponse()
        {
            var req = new RC5EncryptRequestDto
            {
                PlainText = "test",
                Password = "pass",
                KeyBits = 128
            };

            string mockResult = "ENC_DATA|KEY_DATA|HASH_DATA";
            mockService.Setup(s => s.EncryptText(req.PlainText, req.Password, req.KeyBits, It.IsAny<int>(), It.IsAny<int>()))
                       .Returns(mockResult);

            var result = controller.EncryptText(req);
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<RC5EncryptResponseDto>(okResult.Value);

            Assert.Equal("ENC_DATA", response.EncryptedText);
            Assert.Equal("KEY_DATA", response.KeyBase64);
            Assert.Equal("HASH_DATA", response.Md5Hash);
        }

        [Fact]
        public void DecryptText_ShouldReturnOkWithDecryptedString()
        {
            var req = new RC5EncryptResponseDto
            {
                EncryptedText = "ENC",
                KeyBase64 = "KEY",
                Md5Hash = "HASH"
            };
            string combined = "ENC|KEY|HASH";
            string expectedDecrypted = "Original Text";

            mockService.Setup(s => s.DecryptText(combined, It.IsAny<int>(), It.IsAny<int>()))
                       .Returns(expectedDecrypted);

            var result = controller.DecryptText(req);
            var okResult = Assert.IsType<OkObjectResult>(result.Result);

            Assert.NotNull(okResult.Value);
            var prop = okResult.Value.GetType().GetProperty("Decrypted");
            Assert.Equal(expectedDecrypted, prop?.GetValue(okResult.Value));
        }

        [Fact]
        public async Task EncryptFile_ShouldReturnOkWithResponse()
        {
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("doc.txt");

            var req = new RC5FileEncryptRequestDto
            {
                File = fileMock.Object,
                Password = "pass",
                KeyBits = 64
            };

            byte[] encBytes = new byte[] { 1, 2, 3 };
            string md5 = "hash";
            string key = "key";

            mockService.Setup(s => s.EncryptFileAsync(fileMock.Object, req.Password, req.KeyBits, It.IsAny<int>(), It.IsAny<int>()))
                       .ReturnsAsync((encBytes, md5, key));

            var result = await controller.EncryptFile(req);
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<RC5FileEncryptResponseDto>(okResult.Value);

            Assert.Equal("doc.txt", response.FileName);
            Assert.Equal(encBytes, response.EncryptedData);
            Assert.Equal(md5, response.Md5Hash);
            Assert.Equal(key, response.KeyBase64);
        }

        [Fact]
        public async Task DecryptFile_ShouldReturnFileResult()
        {
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("encrypted.bin");

            var req = new RC5FileEncryptRequestDto
            {
                File = fileMock.Object,
                Password = "pass",
                KeyBits = 64
            };

            byte[] decBytes = System.Text.Encoding.UTF8.GetBytes("decrypted content");

            mockService.Setup(s => s.DecryptFileAsync(fileMock.Object, req.Password, req.KeyBits, It.IsAny<int>(), It.IsAny<int>()))
                       .ReturnsAsync(decBytes);

            var result = await controller.DecryptFile(req);
            var fileResult = Assert.IsType<FileContentResult>(result);

            Assert.Equal("application/octet-stream", fileResult.ContentType);
            Assert.Equal(decBytes, fileResult.FileContents);
            Assert.StartsWith("decrypted_", fileResult.FileDownloadName);
        }
    }
}