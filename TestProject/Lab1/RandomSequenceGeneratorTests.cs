using InformationProtection1.Services.Lab1.Implementations;

namespace InformationProtection1.Tests.Lab1.Services
{
    public class RandomSequenceGeneratorServiceTests
    {
        private readonly RandomSequenceGeneratorService service = new();

        [Fact]
        public void GenerateRandomSequence_ShouldReturnCorrectSequence_FullCycle()
        {
            long m = 7, a = 3, c = 4, X0 = 1;
            int amount = 10;
            var expected = new List<long> { 1, 0, 4, 2, 3, 5, 1, 0, 4, 2 };

            List<long>? result = service.GenerateRandomSequence(amount, m, a, c, X0);

            Assert.NotNull(result);
            Assert.Equal(amount, result.Count);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GenerateRandomSequence_ShouldUseDefaultParameters()
        {
            int amount = 3;
            long m = (long)Math.Pow(2, 31) - 3;
            long a = (long)Math.Pow(2, 15);
            long c = 46368;
            long X0 = 37;

            long X1 = (a * X0 + c) % m;
            long X2 = (a * X1 + c) % m;

            var expected = new List<long> { X0, X1, X2 };

            List<long>? result = service.GenerateRandomSequence(amount);

            Assert.NotNull(result);
            Assert.Equal(expected.Count, result.Count);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GeneratePartOfRandomSequence_ShouldReturnCorrectSubSequence()
        {
            long m = 7, a = 3, c = 4, X0 = 1;
            int amount = 10;
            int start = 2, end = 5;
            var expected = new List<long> { 4, 2, 3 };

            List<long>? result = service.GeneratePartOfRandomSequence(amount, start, end, m, a, c, X0);

            Assert.NotNull(result);
            Assert.Equal(end - start, result.Count);
            Assert.Equal(expected, result);
        }
    }
}