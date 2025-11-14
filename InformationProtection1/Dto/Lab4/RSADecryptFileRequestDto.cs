namespace InformationProtection1.Dto.Lab4
{
    public class RSADecryptFileRequestDto
    {
        public IFormFile EncryptedFile { get; set; } = default!;
        public IFormFile PrivateKeyFile { get; set; } = default!;
    }
}
