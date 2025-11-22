using InformationProtection1.Services.Lab2.Implementations;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Text;

namespace InformationProtection1.Tests.Lab2.Services
{
    public class Md5HashServiceTests
    {
        private readonly Md5HashService service = new();

        [Theory]
        [InlineData("", "D41D8CD98F00B204E9800998ECF8427E")]
        [InlineData("a", "0CC175B9C0F1B6A831C399E269772661")]
        [InlineData("abc", "900150983CD24FB0D6963F7D28E17F72")]
        [InlineData("message digest", "F96B697D7CB7938D525A2F31AAF161D0")]
        [InlineData("abcdefghijklmnopqrstuvwxyz", "C3FCD3D76192E4007DFB496CCA67E13B")]
        [InlineData("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789", "D174AB98D277D9F5A5611C2C9F419D9F")]
        [InlineData("12345678901234567890123456789012345678901234567890123456789012345678901234567890", "57EDF4A22BE3C955AC49DA2E2107B67A")]
        public void ComputeMd5_ShouldReturnCorrectHash_ForTestVectors(string input, string expected)
        {
            string result = service.ComputeMd5(input);
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task ComputeFileMd5Async_ShouldReturnCorrectHash()
        {
            string content = "abc";
            string expectedHash = "900150983CD24FB0D6963F7D28E17F72";
            var fileMock = CreateMockFile(content);

            string result = await service.ComputeFileMd5Async(fileMock.Object);

            Assert.Equal(expectedHash, result);
        }

        [Fact]
        public async Task VerifyFileIntegrityAsync_ShouldReturnTrue_WhenHashesMatch()
        {
            string content = "abc";
            string expectedHash = "900150983CD24FB0D6963F7D28E17F72";
            var fileMock = CreateMockFile(content);

            bool result = await service.VerifyFileIntegrityAsync(fileMock.Object, expectedHash);

            Assert.True(result);
        }

        [Fact]
        public async Task VerifyFileIntegrityAsync_ShouldReturnFalse_WhenHashesMismatch()
        {
            string content = "abc";
            string wrongHash = "INVALIDHASH12345";
            var fileMock = CreateMockFile(content);

            bool result = await service.VerifyFileIntegrityAsync(fileMock.Object, wrongHash);

            Assert.False(result);
        }

        private Mock<IFormFile> CreateMockFile(string content)
        {
            var fileMock = new Mock<IFormFile>();
            var bytes = Encoding.UTF8.GetBytes(content);

            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Callback<Stream, CancellationToken>((stream, token) =>
                {
                    stream.Write(bytes, 0, bytes.Length);
                })
                .Returns(Task.CompletedTask);

            fileMock.Setup(f => f.Length).Returns(bytes.Length);
            return fileMock;
        }
    }
}