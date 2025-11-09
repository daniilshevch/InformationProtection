namespace InformationProtection1.Dto.Lab3
{
    public class RC5FileEncryptResponseDto
    {
        public string FileName { get; set; } = string.Empty;
        public byte[] EncryptedData { get; set; } = Array.Empty<byte>();
        public string Md5Hash { get; set; } = string.Empty;
        public string KeyBase64 { get; set; } = string.Empty;
    }
}
