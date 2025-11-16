namespace InformationProtection1.Services.Lab4.Interfaces
{
    public interface IRSAService
    {
        Task DecryptStreamAsync(Stream inputStream, string privateKeyPem, Stream outputStream);
        Task EncryptStreamAsync(Stream inputStream, string publicKeyPem, Stream outputStream);
        (string publicKeyPem, string privateKeyPem) GenerateKeys();
    }
}