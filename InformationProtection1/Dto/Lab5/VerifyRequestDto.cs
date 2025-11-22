namespace InformationProtection1.Dto.Lab5
{
    public class VerifyRequestDto
    {
        public string Data { get; set; } = string.Empty;
        public string SignatureHex { get; set; } = string.Empty;
        public string PublicKey { get; set; } = string.Empty;
    }
}
