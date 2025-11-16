using InformationProtection1.Dto.Lab1;

namespace InformationProtection1.Services.Lab1.Interfaces
{
    public interface IPeriodCheckerService
    {
        PeriodResultDto BrentAlgorithm(long? _m = null, long? _a = null, long? _c = null, long? _X0 = null);
        long Next(long x, long m, long a, long c);
    }
}