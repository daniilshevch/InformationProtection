namespace InformationProtection1.Services.Lab3.Interfaces
{
    public interface IRC5EncryptionService
    {
        Task<byte[]> DecryptFileAsync(IFormFile file, string password, int keyBits, int w = 32, int r = 12);
        string DecryptText(string combined, int w = 32, int r = 12);
        byte[] DeriveKeyFromPassword(string password, int keyBits);
        Task<(byte[] Encrypted, string Md5, string KeyBase64)> EncryptFileAsync(IFormFile file, string password, int keyBits, int w = 32, int r = 12);
        string EncryptText(string plainText, string password, int keyBits, int w = 32, int r = 12);
    }
}