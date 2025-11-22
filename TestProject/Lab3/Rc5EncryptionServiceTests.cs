using InformationProtection1.Services.Lab3.Implementations;
using InformationProtection1.Services.Lab1.Interfaces;
using InformationProtection1.Services.Lab2.Interfaces;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Text;

namespace InformationProtection1.Tests.Lab3.Services
{
    public class RC5EncryptionServiceTests
    {
        private readonly Mock<IRandomSequenceGeneratorService> mockRandom = new();
        private readonly Mock<IMd5HashService> mockMd5 = new();
        private readonly RC5EncryptionService service;

        public RC5EncryptionServiceTests()
        {
            mockMd5.Setup(m => m.ComputeMd5(It.IsAny<string>()))
                   .Returns("D41D8CD98F00B204E9800998ECF8427E"); 

            var fixedSequence = new List<long> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
            mockRandom.Setup(r => r.GenerateRandomSequence(It.IsAny<int>(), null, null, null, null))
                      .Returns(fixedSequence);

            service = new RC5EncryptionService(mockRandom.Object, mockMd5.Object);
        }

        [Fact]
        public void RotateLeft_ShouldRotateCorrectly()
        {
            ulong value = 0x1234567890ABCDEF;
            ulong expected = 0x234567890ABCDEF1;
            ulong result = RC5EncryptionService.RotateLeft(value, 4);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void RotateRight_ShouldRotateCorrectly()
        {
            ulong value = 0x1234567890ABCDEF;
            ulong expected = 0xF1234567890ABCDE;
            ulong result = RC5EncryptionService.RotateRight(value, 4);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void Padding_ShouldAddAndRemoveCorrectly()
        {
            byte[] data = new byte[] { 1, 2, 3 }; 
            byte[] padded = RC5EncryptionService.ApplyPadding(data);

            Assert.Equal(16, padded.Length);
            Assert.Equal(13, padded[15]); 

            byte[] unpadded = RC5EncryptionService.RemovePadding(padded);

            Assert.Equal(data.Length, unpadded.Length);
            Assert.Equal(data, unpadded);
        }

        [Fact]
        public void EncryptDecryptText_ShouldReturnOriginalText()
        {
            string plainText = "Hello World";
            string password = "secretpassword";
            int keyBits = 128;

            mockMd5.Setup(m => m.ComputeMd5(plainText)).Returns("dummyhash");
            mockMd5.Setup(m => m.ComputeMd5(password)).Returns("5F4DCC3B5AA765D61D8327DEB882CF99"); 

            string encryptedPayload = service.EncryptText(plainText, password, keyBits);
            string decryptedText = service.DecryptText(encryptedPayload);

            Assert.Equal(plainText, decryptedText);
        }

        [Fact]
        public async Task EncryptDecryptFile_ShouldReturnOriginalData()
        {
            string content = "This is a test file content for RC5 encryption.";
            var fileMock = CreateMockFile(content, "test.txt");
            string password = "filepassword";
            int keyBits = 128;

            mockMd5.Setup(m => m.ComputeMd5(password)).Returns("5F4DCC3B5AA765D61D8327DEB882CF99");
            mockMd5.Setup(m => m.ComputeMd5Bytes(It.IsAny<byte[]>())).Returns("filehash");

            var result = await service.EncryptFileAsync(fileMock.Object, password, keyBits);

            var encryptedFileMock = new Mock<IFormFile>();
            var encryptedStream = new MemoryStream(result.Encrypted);
            encryptedFileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Callback<Stream, CancellationToken>((stream, token) =>
                {
                    encryptedStream.Position = 0;
                    encryptedStream.CopyTo(stream);
                })
                .Returns(Task.CompletedTask);
            encryptedFileMock.Setup(f => f.Length).Returns(result.Encrypted.Length);

            byte[] decryptedBytes = await service.DecryptFileAsync(encryptedFileMock.Object, password, keyBits);
            string decryptedContent = Encoding.UTF8.GetString(decryptedBytes);

            Assert.Equal(content, decryptedContent);
        }

        [Fact]
        public void DetectFileExtension_ShouldIdentifyTypes()
        {
            byte[] png = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x00, 0x00, 0x00, 0x00 };
            Assert.Equal(".png", RC5EncryptionService.DetectFileExtension(png));

            byte[] jpg = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 };
            Assert.Equal(".jpg", RC5EncryptionService.DetectFileExtension(jpg));

            byte[] unknown = new byte[] { 0x00, 0x01, 0x02 };
            Assert.Equal(".bin", RC5EncryptionService.DetectFileExtension(unknown));
        }

        private Mock<IFormFile> CreateMockFile(string content, string fileName)
        {
            var fileMock = new Mock<IFormFile>();
            var bytes = Encoding.UTF8.GetBytes(content);

            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(bytes.Length);
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Callback<Stream, CancellationToken>((stream, token) =>
                {
                    stream.Write(bytes, 0, bytes.Length);
                })
                .Returns(Task.CompletedTask);

            return fileMock;
        }
    }
}