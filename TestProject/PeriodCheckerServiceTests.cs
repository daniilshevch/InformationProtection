using Xunit;
using InformationProtection1.Services.Lab1.Implementations;

namespace InformationProtection1.Tests.Lab1
{
    public class PeriodCheckerServiceTests
    {
        private readonly PeriodCheckerService _checker;

        public PeriodCheckerServiceTests()
        {
            _checker = new PeriodCheckerService();
        }

        [Fact]
        public void BrentAlgorithm_ShouldFindPeriodAndPrefix_ForKnownSequence()
        {
            long m = 10, a = 3, c = 1, X0 = 1;
            long expectedPrefix = 0;
            long expectedPeriod = 4;

            PeriodResultDto result = _checker.BrentAlgorithm(m, a, c, X0);

            Assert.Equal(expectedPrefix, result.prefix);
            Assert.Equal(expectedPeriod, result.period);
        }

        [Fact]
        public void BrentAlgorithm_ShouldFindPeriodAndPrefix_ForSequenceWithPrefix()
        {
            long m = 14, a = 3, c = 5, X0 = 1;
            long expectedPrefix = 2;
            long expectedPeriod = 6;

            PeriodResultDto result = _checker.BrentAlgorithm(m, a, c, X0);

            Assert.Equal(expectedPrefix, result.prefix);
            Assert.Equal(expectedPeriod, result.period);
        }
    }
}