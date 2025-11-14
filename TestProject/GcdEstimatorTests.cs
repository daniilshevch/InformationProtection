using Xunit;
using InformationProtection1.Services.Lab1;

namespace InformationProtection1.Tests.Lab1
{
    public class GcdEstimatorTests
    {
        private readonly GcdEstimator _estimator;

        public GcdEstimatorTests()
        {
            _estimator = new GcdEstimator();
        }

        [Theory]
        [InlineData(48, 18, 6)]
        [InlineData(18, 48, 6)]
        [InlineData(17, 5, 1)]
        [InlineData(10, 0, 10)]
        [InlineData(0, 5, 5)]
        [InlineData(0, 0, 0)]
        [InlineData(1, 1, 1)]
        [InlineData(-48, 18, 6)]
        [InlineData(48, -18, 6)]
        [InlineData(-48, -18, 6)]
        public void CountGCD_ShouldReturnCorrectGcd(long a, long b, long expected)
        {
            long result = _estimator.CountGCD(a, b);
            Assert.Equal(expected, result);
        }
    }
}