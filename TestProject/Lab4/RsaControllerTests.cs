using InformationProtection1.Controllers.Lab4;
using InformationProtection1.Dto.Lab4;
using InformationProtection1.Services.Lab4.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Cryptography;
using System.Text;

namespace InformationProtection1.Tests.Lab4.Controllers
{
    public class RSAControllerTests
    {
        private readonly Mock<IRSAService> mockService = new();
        private readonly RSAController controller;

        public RSAControllerTests()
        {
            controller = new RSAController(mockService.Object);
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        [Fact]
        public void GenerateKeysAndDownload_ShouldReturnZipFile()
        {
            string mockPub = "PUBLIC_PEM";
            string mockPriv = "PRIVATE_PEM";
            mockService.Setup(s => s.GenerateKeys()).Returns((mockPub, mockPriv));

            var result = controller.GenerateKeysAndDownload();

            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/zip", fileResult.ContentType);
            Assert.Equal("rsa_keys.zip", fileResult.FileDownloadName);
            Assert.True(fileResult.FileContents.Length > 0);
        }

        [Fact]
        public async Task EncryptFile_ShouldReturnEncryptedFile_And_TimeHeader()
        {
            var inputFileMock = CreateMockFile("input.txt", "content");
            var keyFileMock = CreateMockFile("pub.pem", "PUBLIC_KEY");

            var request = new RSAEncryptFileRequestDto
            {
                InputFile = inputFileMock.Object,
                PublicKeyFile = keyFileMock.Object
            };

            long expectedTime = 123;
            mockService.Setup(s => s.EncryptStreamAsync(
                It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<Stream>()))
                .ReturnsAsync(expectedTime);

            var result = await controller.EncryptFile(request);

            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("encrypted_input.txt.dat", fileResult.FileDownloadName);

            Assert.True(controller.Response.Headers.ContainsKey("X-Encryption-Time-Ms"));
            Assert.Equal(expectedTime.ToString(), controller.Response.Headers["X-Encryption-Time-Ms"]);
        }

        [Fact]
        public async Task EncryptFile_ShouldReturnBadRequest_OnBadKeyFormat()
        {
            var inputFileMock = CreateMockFile("input.txt", "content");
            var keyFileMock = CreateMockFile("bad.pem", "INVALID_KEY");

            var request = new RSAEncryptFileRequestDto
            {
                InputFile = inputFileMock.Object,
                PublicKeyFile = keyFileMock.Object
            };

            mockService.Setup(s => s.EncryptStreamAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<Stream>()))
                .ThrowsAsync(new ArgumentException("Bad PEM"));

            var result = await controller.EncryptFile(request);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Некоректний формат публічного ключа", badRequest.Value!.ToString());
        }

        [Fact]
        public async Task DecryptFile_ShouldReturnDecryptedFile()
        {
            var encFileMock = CreateMockFile("encrypted_file.dat", "ENCRYPTED_DATA");
            var keyFileMock = CreateMockFile("priv.pem", "PRIVATE_KEY");

            var request = new RSADecryptFileRequestDto
            {
                EncryptedFile = encFileMock.Object,
                PrivateKeyFile = keyFileMock.Object
            };

            mockService.Setup(s => s.DecryptStreamAsync(
                It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<Stream>()))
                .ReturnsAsync(100); 

            var result = await controller.DecryptFile(request);

            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("decrypted_encrypted_file", fileResult.FileDownloadName);
        }

        [Fact]
        public async Task DecryptFile_ShouldReturnBadRequest_OnCryptographicException()
        {
            var encFileMock = CreateMockFile("data.bin", "DATA");
            var keyFileMock = CreateMockFile("key.pem", "KEY");

            var request = new RSADecryptFileRequestDto
            {
                EncryptedFile = encFileMock.Object,
                PrivateKeyFile = keyFileMock.Object
            };

            mockService.Setup(s => s.DecryptStreamAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<Stream>()))
                .ThrowsAsync(new CryptographicException("Padding is invalid"));

            var result = await controller.DecryptFile(request);
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Помилка дешифрування", badRequest.Value!.ToString());
        }

        private Mock<IFormFile> CreateMockFile(string fileName, string content)
        {
            var fileMock = new Mock<IFormFile>();
            var bytes = Encoding.UTF8.GetBytes(content);
            var ms = new MemoryStream(bytes);

            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(bytes.Length);
            fileMock.Setup(f => f.OpenReadStream()).Returns(ms);
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Callback<Stream, CancellationToken>((stream, token) =>
                {
                    ms.Position = 0;
                    ms.CopyTo(stream);
                })
                .Returns(Task.CompletedTask);

            return fileMock;
        }
    }
}