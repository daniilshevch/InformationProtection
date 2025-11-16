using System.Numerics;
using InformationProtection1.Dto.Lab1;
using InformationProtection1.Services.Lab1.Interfaces;
namespace InformationProtection1.Services.Lab1.Implementations
{

    public class PeriodCheckerService : IPeriodCheckerService
    {
        public long Next(long x, long m, long a, long c)
        {
            return (long)(((BigInteger)a * x + c) % m);
        }
        public PeriodResultDto BrentAlgorithm(long? _m = null, long? _a = null, long? _c = null, long? _X0 = null)
        {
            long m = _m ?? (long)Math.Pow(2, 31) - 3;
            long a = _a ?? (long)Math.Pow(2, 15);
            long c = _c ?? 46368;
            long X0 = _X0 ?? 37;
            long power = 1, lam = 1;
            long tort = X0, hare = Next(X0, m, a, c);
            while (tort != hare)
            {
                if (power == lam)
                {
                    tort = hare;
                    power <<= 1;
                    lam = 0;
                }
                hare = Next(hare, m, a, c);
                lam++;
            }
            long prefix = 0;
            tort = hare = X0;
            for (int i = 0; i < lam; i++)
            {
                hare = Next(hare, m, a, c);
            }
            while (tort != hare)
            {
                tort = Next(tort, m, a, c);
                hare = Next(hare, m, a, c);
                prefix++;
            }
            return new PeriodResultDto(prefix, lam);
        }
    }
}
