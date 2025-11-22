using InformationProtection1.Dto.Lab5;

namespace InformationProtection1.Services.Lab5.Interfaces
{
    public interface IDsaService
    {
        DsaKeysDto GenerateKeys();
        string SignData(string data, string privateKeyPem);
        string SignFile(Stream fileStream, string privateKeyPem);
        bool VerifyData(string data, string signatureHex, string publicKeyPem);
        bool VerifyFile(Stream fileStream, string signatureHex, string publicKeyPem);
    }
}
