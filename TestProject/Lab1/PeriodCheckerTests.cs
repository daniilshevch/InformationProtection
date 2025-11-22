using InformationProtection1.Services.Lab1.Implementations;
using InformationProtection1.Dto.Lab1;

namespace InformationProtection1.Tests.Lab1.Services
{
    public class PeriodCheckerServiceTests
    {
        private readonly PeriodCheckerService service = new();

        [Fact]
        public void Next_ShouldReturnCorrectNextValue()
        {
            long m = 10, a = 7, c = 5, x = 3;
            long expected = (7 * 3 + 5) % 10; 
            long result = service.Next(x, m, a, c);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void BrentAlgorithm_ShouldFindFullPeriod()
        {
            long m = 8, a = 5, c = 1, X0 = 1;

            PeriodResultDto result = service.BrentAlgorithm(m, a, c, X0);

            Assert.Equal(0, result.prefix);
            Assert.Equal(8, result.period);
        }

        [Fact]
        public void BrentAlgorithm_ShouldFindCorrectPeriodWithPrefix()
        {
            long m = 10, a = 1, c = 0, X0 = 5;

            PeriodResultDto result = service.BrentAlgorithm(m, a, c, X0);

            Assert.Equal(1, result.prefix);
            Assert.Equal(1, result.period);
        }

        [Fact]
        public void BrentAlgorithm_ShouldUseDefaultParameters()
        {
            PeriodResultDto result = service.BrentAlgorithm();
            Assert.True(result.period > 0);
            Assert.True(result.prefix >= 0);
        }
    }
}