namespace InformationProtection1.Dto.Lab2
{
    public class FileIntegrityCheckDto
    {
        public IFormFile File { get; set; } = default!;
        public string ExpectedMd5Hash { get; set; } = string.Empty;
    }
}
