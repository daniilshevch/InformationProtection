namespace InformationProtection1.Dto.Lab1
{
    public class PeriodResultDto
    {
        public long prefix { get; set; }
        public long period { get; set; }
        public PeriodResultDto(long prefix, long period)
        {
            this.prefix = prefix;
            this.period = period;
        }
    }
}
