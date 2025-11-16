using InformationProtection1.Dto.Lab1;
using InformationProtection1.Services.Lab1.Interfaces;
using System.Transactions;

namespace InformationProtection1.Services.Lab1.Implementations
{
    public class CesaroTesterService : ICesaroTesterService
    {
        private readonly IGcdEstimator gcdEstimator;
        private readonly IRandomSequenceGeneratorService randomSequenceGeneratorService;
        public CesaroTesterService(IGcdEstimator gcdEstimator,
            IRandomSequenceGeneratorService randomSequenceGeneratorService)
        {
            this.gcdEstimator = gcdEstimator;
            this.randomSequenceGeneratorService = randomSequenceGeneratorService;
        }
        public CesaroResultDto EstimatePiWithSequence(List<long> sequence)
        {
            long pairs = 0, coprime = 0;
            for (int i = 0; i + 1 < sequence.Count; i += 2)
            {
                pairs++;
                if (gcdEstimator.CountGCD(sequence[i], sequence[i + 1]) == 1)
                {
                    coprime++;
                }
            }
            double probability = pairs == 0 ? double.NaN : (double)coprime / pairs;
            double pi = double.IsNaN(probability) ? double.NaN : Math.Sqrt(6.0 / probability);
            return new CesaroResultDto(pi, probability, pairs, coprime);
        }
        public CesaroResultDto? EstimatePi(int amount, long? m = null, long? a = null, long? c = null, long? X0 = null)
        {
            List<long>? sequence = randomSequenceGeneratorService.GenerateRandomSequence(
                amount: amount, _m: m, _a: a, _c: c, _X0: X0);
            if (sequence is null)
            {
                return null;
            }
            return EstimatePiWithSequence(sequence);
        }
    }
}
