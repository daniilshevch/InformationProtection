
namespace InformationProtection1.Services.Lab2
{
    public interface IMd5HashService
    {
        Task<string> ComputeFileMd5Async(IFormFile file);
        string ComputeMd5(string input);
        string ComputeMd5Bytes(byte[] message);
        Task<bool> VerifyFileIntegrityAsync(IFormFile file, string expectedMd5);
    }
}