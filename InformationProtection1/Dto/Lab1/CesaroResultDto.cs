namespace InformationProtection1.Dto.Lab1
{
    public class CesaroResultDto
    {
        public double pi { get; set; }
        public double probability { get; set; }
        public long pairs { get; set; }
        public long coprime { get; set; }
        public CesaroResultDto(double pi, double probability, long pairs, long coprime)
        {
            this.pi = pi;
            this.probability = probability;
            this.pairs = pairs;
            this.coprime = coprime;
        }
    }
}
