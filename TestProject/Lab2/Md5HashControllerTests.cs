using InformationProtection1.Controllers.Lab2;
using InformationProtection1.Dto.General;
using InformationProtection1.Dto.Lab2;
using InformationProtection1.Services.Lab2.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace InformationProtection1.Tests.Lab2.Controllers
{
    public class Md5HashControllerTests
    {
        private readonly Mock<IMd5HashService> mockService = new();
        private readonly Md5HashController controller;

        public Md5HashControllerTests()
        {
            controller = new Md5HashController(mockService.Object);
        }

        [Fact]
        public void ComputeTextHash_ShouldReturnOkWithHash()
        {
            string input = "test";
            string expectedHash = "HASH123";
            var dto = new HashRequestDto { InputText = input };

            mockService.Setup(s => s.ComputeMd5(input)).Returns(expectedHash);

            var result = controller.ComputeTextHash(dto);
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<HashResponseDto>(okResult.Value);

            Assert.Equal(input, response.Input);
            Assert.Equal(expectedHash, response.Md5Hash);
        }

        [Fact]
        public async Task ComputeFileHash_ShouldReturnOkWithHashAndTiming()
        {
            string expectedHash = "FILEHASH123";
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("test.txt");
            var dto = new FileUploadDto { File = fileMock.Object };

            mockService.Setup(s => s.ComputeFileMd5Async(fileMock.Object))
                .ReturnsAsync(expectedHash);

            var result = await controller.ComputeFileHash(dto);
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<HashResponseDto>(okResult.Value);

            Assert.Equal("test.txt", response.Input);
            Assert.Equal(expectedHash, response.Md5Hash);
            Assert.NotNull(response.GenerationTime);
        }

        [Fact]
        public async Task VerifyFile_ShouldReturnOkWithSuccessMessage()
        {
            string expectedHash = "HASH";
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("test.txt");
            var dto = new FileIntegrityCheckDto { File = fileMock.Object, ExpectedMd5Hash = expectedHash };

            mockService.Setup(s => s.VerifyFileIntegrityAsync(fileMock.Object, expectedHash))
                .ReturnsAsync(true);

            var result = await controller.VerifyFile(dto);
            var okResult = Assert.IsType<OkObjectResult>(result);

            var resultValue = okResult.Value!.ToString();
            Assert.Contains("Файл цілісний", resultValue);
        }

        [Fact]
        public async Task VerifyFile_ShouldReturnOkWithFailureMessage()
        {
            string expectedHash = "HASH";
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("test.txt");
            var dto = new FileIntegrityCheckDto { File = fileMock.Object, ExpectedMd5Hash = expectedHash };

            mockService.Setup(s => s.VerifyFileIntegrityAsync(fileMock.Object, expectedHash))
                .ReturnsAsync(false);

            var result = await controller.VerifyFile(dto);
            var okResult = Assert.IsType<OkObjectResult>(result);

            var resultValue = okResult.Value!.ToString();
            Assert.Contains("Хеш не співпадає", resultValue);
        }

        [Fact]
        public void TestVectors_ShouldReturnOkWithResults()
        {
            string input = "abc";
            string expectedHash = "HASH_ABC";
            mockService.Setup(s => s.ComputeMd5(input)).Returns(expectedHash);

            var result = controller.TestVectors();
            var okResult = Assert.IsType<OkObjectResult>(result);

            Assert.NotNull(okResult.Value);
        }
    }
}