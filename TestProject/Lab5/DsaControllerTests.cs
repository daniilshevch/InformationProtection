using InformationProtection1.Dto.Lab5;
using InformationProtection1.Services.Lab5.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.IO;
using Xunit;

namespace SecurityLab5.Tests
{
    public class DigitalSignatureControllerTests
    {
        private readonly Mock<IDsaService> _mockService;
        private readonly DSAController _controller;

        public DigitalSignatureControllerTests()
        {
            _mockService = new Mock<IDsaService>();
            _controller = new DSAController(_mockService.Object);
        }

        [Fact]
        public void GenerateKeys_ReturnsOk_WithKeys()
        {
            var expectedKeys = new DsaKeysDto { PrivateKey = "priv", PublicKey = "pub" };
            _mockService.Setup(s => s.GenerateKeys()).Returns(expectedKeys);
            var result = _controller.GenerateKeys();
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnDto = Assert.IsType<DsaKeysDto>(okResult.Value);
            Assert.Equal("priv", returnDto.PrivateKey);
        }

        [Fact]
        public void SignText_ReturnsOk_WhenServiceSucceeds()
        {
            var request = new SignRequestDto { Data = "test", PrivateKey = "key" };
            _mockService.Setup(s => s.SignData("test", "key")).Returns("AABBCC");
            var result = _controller.SignText(request);
            var okResult = Assert.IsType<OkObjectResult>(result);
            dynamic val = okResult.Value;
            Assert.Equal("AABBCC", (string)val.GetType().GetProperty("SignatureHex").GetValue(val, null));
        }

        [Fact]
        public void SignText_ReturnsBadRequest_WhenServiceThrowsException()
        {
            var request = new SignRequestDto { Data = "test", PrivateKey = "bad_key" };
            _mockService.Setup(s => s.SignData(It.IsAny<string>(), It.IsAny<string>()))
                        .Throws(new System.Exception("Invalid Key"));
            var result = _controller.SignText(request);
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Error signing data", badRequest.Value.ToString());
        }

        [Fact]
        public void SignFile_ReturnsOk_WithSignature()
        {
            var mockFile = new Mock<IFormFile>();
            var content = "Hello World from File";
            var ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
            mockFile.Setup(f => f.OpenReadStream()).Returns(ms);
            mockFile.Setup(f => f.Length).Returns(ms.Length);
            _mockService.Setup(s => s.SignFile(It.IsAny<Stream>(), "key")).Returns("112233");
            var result = _controller.SignFile(mockFile.Object, "key");
            var okResult = Assert.IsType<OkObjectResult>(result);
            dynamic val = okResult.Value;
            Assert.Equal("112233", (string)val.GetType().GetProperty("SignatureHex").GetValue(val, null));
        }

        [Fact]
        public void VerifyText_ReturnsOk_WithValidationResult()
        {
            var request = new VerifyRequestDto { Data = "data", SignatureHex = "sig", PublicKey = "pub" };
            _mockService.Setup(s => s.VerifyData("data", "sig", "pub")).Returns(true);

            var result = _controller.VerifyText(request);

            var okResult = Assert.IsType<OkObjectResult>(result);
            dynamic val = okResult.Value;
            Assert.True((bool)val.GetType().GetProperty("IsValid").GetValue(val, null));
        }
    }
}