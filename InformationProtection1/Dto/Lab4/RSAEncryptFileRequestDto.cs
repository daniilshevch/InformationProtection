namespace InformationProtection1.Dto.Lab4
{
    public class RSAEncryptFileRequestDto
    {
        public IFormFile InputFile { get; set; } = default!;
        public IFormFile PublicKeyFile { get; set; } = default!;
    }
}
