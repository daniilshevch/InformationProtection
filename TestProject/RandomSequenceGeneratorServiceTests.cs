using Xunit;
using InformationProtection1.Services.Lab1;
using System.Collections.Generic;

namespace InformationProtection1.Tests.Lab1
{
    public class RandomSequenceGeneratorServiceTests
    {
        private readonly RandomSequenceGeneratorService _generator;

        public RandomSequenceGeneratorServiceTests()
        {
            _generator = new RandomSequenceGeneratorService();
        }

        [Fact]
        public void GenerateRandomSequence_ShouldReturnCorrectAmount()
        {
            int amount = 10;
            List<long> sequence = _generator.GenerateRandomSequence(amount);
            Assert.NotNull(sequence);
            Assert.Equal(amount, sequence.Count);
        }

        [Fact]
        public void GenerateRandomSequence_ShouldStartWithX0()
        {
            long x0 = 12345;
            List<long> sequence = _generator.GenerateRandomSequence(10, _X0: x0);
            Assert.Equal(x0, sequence[0]);
        }

        [Fact]
        public void GenerateRandomSequence_ShouldGenerateKnownSequence()
        {
            int amount = 5;
            long m = 100;
            long a = 3;
            long c = 7;
            long X0 = 1;
            var expectedSequence = new List<long> { 1, 10, 37, 18, 61 };

            List<long> sequence = _generator.GenerateRandomSequence(amount, m, a, c, X0);

            Assert.Equal(expectedSequence, sequence);
        }

        [Fact]
        public void GeneratePartOfRandomSequence_ShouldReturnCorrectSlice()
        {
            int amount = 5;
            long m = 100, a = 3, c = 7, X0 = 1;
            int start = 1;
            int end = 4;
            var expectedSlice = new List<long> { 10, 37, 18 };

            List<long> slice = _generator.GeneratePartOfRandomSequence(amount, start, end, m, a, c, X0);

            Assert.Equal(expectedSlice, slice);
        }

        [Fact]
        public void GenerateRandomSequence_ShouldUseDefaultParameters()
        {
            int amount = 2;
            var expectedSequence = new List<long> { 37, 1258784 };

            List<long> sequence = _generator.GenerateRandomSequence(amount, null, null, null, null);

            Assert.Equal(expectedSequence, sequence);
        }
    }
}