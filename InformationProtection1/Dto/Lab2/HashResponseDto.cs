namespace InformationProtection1.Dto.Lab2
{
    public class HashResponseDto
    {
        public string Input { get; set; } = string.Empty;
        public string Md5Hash { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        public TimeSpan GenerationTime { get; set; }
    }
}
