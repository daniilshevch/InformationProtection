using InformationProtection1.Dto.Lab5;
using InformationProtection1.Services.Lab5.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace InformationProtection1.Services.Lab5.Implementations
{

    public class DsaService : IDsaService
    {
        public DsaKeysDto GenerateKeys()
        {
            using (DSA dsa = DSA.Create())
            {
                dsa.KeySize = 2048; 
                var privateKey = dsa.ExportPkcs8PrivateKeyPem();
                var publicKey = dsa.ExportSubjectPublicKeyInfoPem();

                return new DsaKeysDto
                {
                    PrivateKey = privateKey,
                    PublicKey = publicKey
                };
            }
        }

        public string SignData(string data, string privateKeyPem)
        {
            var dataBytes = Encoding.UTF8.GetBytes(data);
            var signatureBytes = SignBytes(dataBytes, privateKeyPem);
            return Convert.ToHexString(signatureBytes);
        }

        public string SignFile(Stream fileStream, string privateKeyPem)
        {
            using (var memoryStream = new MemoryStream())
            {
                fileStream.CopyTo(memoryStream);
                var dataBytes = memoryStream.ToArray();
                var signatureBytes = SignBytes(dataBytes, privateKeyPem);
                return Convert.ToHexString(signatureBytes);
            }
        }

        private byte[] SignBytes(byte[] data, string privateKeyPem)
        {
            using (DSA dsa = DSA.Create())
            {
                dsa.ImportFromPem(privateKeyPem);
                return dsa.SignData(data, HashAlgorithmName.SHA256);
            }
        }

        public bool VerifyData(string data, string signatureHex, string publicKeyPem)
        {
            var dataBytes = Encoding.UTF8.GetBytes(data);
            var signatureBytes = Convert.FromHexString(signatureHex);
            return VerifyBytes(dataBytes, signatureBytes, publicKeyPem);
        }

        public bool VerifyFile(Stream fileStream, string signatureHex, string publicKeyPem)
        {
            using (var memoryStream = new MemoryStream())
            {
                fileStream.CopyTo(memoryStream);
                var dataBytes = memoryStream.ToArray();
                var signatureBytes = Convert.FromHexString(signatureHex);
                return VerifyBytes(dataBytes, signatureBytes, publicKeyPem);
            }
        }

        private bool VerifyBytes(byte[] data, byte[] signature, string publicKeyPem)
        {
            using (DSA dsa = DSA.Create())
            {
                dsa.ImportFromPem(publicKeyPem);
                return dsa.VerifyData(data, signature, HashAlgorithmName.SHA256);
            }
        }
    }
}
