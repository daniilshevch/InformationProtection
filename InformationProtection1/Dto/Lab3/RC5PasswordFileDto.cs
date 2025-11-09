namespace InformationProtection1.Dto.Lab3
{
    public class RC5PasswordFileDto
    {
        public IFormFile File { get; set; } = default!;
        public string Password { get; set; } = string.Empty;
        public int KeyBits { get; set; } = 128; 
    }
}
