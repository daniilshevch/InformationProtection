namespace InformationProtection1.Dto.Lab3
{
    public class RC5EncryptResponseDto
    {
        public string EncryptedText { get; set; } = string.Empty;
        public string KeyBase64 { get; set; } = string.Empty;
        public string Md5Hash { get; set; } = string.Empty;
    }
}
