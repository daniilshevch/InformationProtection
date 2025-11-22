using InformationProtection1.Services.Lab1.Implementations;

namespace InformationProtection1.Tests.Lab1.Services
{
    public class GcdEstimatorTests
    {
        [Theory]
        [InlineData(12, 18, 6)]
        [InlineData(101, 100, 1)]
        [InlineData(7, 0, 7)]
        [InlineData(0, 7, 7)]
        [InlineData(21, 35, 7)]
        [InlineData(100, 100, 100)]
        [InlineData(1, 1000, 1)]
        [InlineData(-12, 18, 6)]
        [InlineData(-21, -35, 7)]
        public void CountGCD_ShouldReturnCorrectGCD(long a, long b, long expected)
        {
            var estimator = new GcdEstimator();
            long result = estimator.CountGCD(a, b);
            Assert.Equal(expected, result);
        }
    }
}
