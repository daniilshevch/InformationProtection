namespace InformationProtection1.Services.Lab4.Interfaces
{
    public interface IRSAService
    {
        Task<long> DecryptStreamAsync(Stream inputStream, string privateKeyPem, Stream outputStream);
        Task<long> EncryptStreamAsync(Stream inputStream, string publicKeyPem, Stream outputStream);
        (string publicKeyPem, string privateKeyPem) GenerateKeys();
    }
}