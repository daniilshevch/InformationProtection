namespace InformationProtection1.Dto.Lab3
{
    public class RC5EncryptRequestDto
    {
        public string PlainText { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int KeyBits { get; set; } = 128;
    }
}
