using Xunit;
using Moq;
using InformationProtection1.Services.Lab1;
using InformationProtection1.Dto.Lab1;
using System.Collections.Generic;

namespace InformationProtection1.Tests.Lab1
{
    public class CesaroTesterServiceTests
    {
        private readonly Mock<GcdEstimator> _mockGcdEstimator;
        private readonly Mock<RandomSequenceGeneratorService> _mockGenerator;
        private readonly CesaroTesterService _service;

        public CesaroTesterServiceTests()
        {
            _mockGcdEstimator = new Mock<GcdEstimator>();
            _mockGenerator = new Mock<RandomSequenceGeneratorService>();
            _service = new CesaroTesterService(_mockGcdEstimator.Object, _mockGenerator.Object);
        }

        [Fact]
        public void EstimatePiWithSequence_ShouldCalculateCorrectly()
        {
            var sequence = new List<long> { 1, 5, 2, 4, 17, 1 };

            _mockGcdEstimator.Setup(g => g.CountGCD(1, 5)).Returns(1);
            _mockGcdEstimator.Setup(g => g.CountGCD(2, 4)).Returns(2);
            _mockGcdEstimator.Setup(g => g.CountGCD(17, 1)).Returns(1);

            long expectedPairs = 3;
            long expectedCoprime = 2;
            double expectedProbability = (double)2 / 3;
            double expectedPi = Math.Sqrt(6.0 / expectedProbability);

            CesaroResultDto result = _service.EstimatePiWithSequence(sequence);

            Assert.NotNull(result);
            Assert.Equal(expectedPairs, result.pairs);
            Assert.Equal(expectedCoprime, result.coprime);
            Assert.Equal(expectedProbability, result.probability);
            Assert.Equal(expectedPi, result.pi);
        }

        [Fact]
        public void EstimatePiWithSequence_ShouldHandleEmptySequence()
        {
            var sequence = new List<long>();

            CesaroResultDto result = _service.EstimatePiWithSequence(sequence);

            Assert.Equal(0, result.pairs);
            Assert.Equal(0, result.coprime);
            Assert.True(double.IsNaN(result.probability));
            Assert.True(double.IsNaN(result.pi));
        }

        [Fact]
        public void EstimatePi_ShouldCallGeneratorAndReturnResult()
        {
            int amount = 4;
            var generatedSequence = new List<long> { 1, 2, 3, 4 };
            _mockGenerator.Setup(g => g.GenerateRandomSequence(amount, null, null, null, null))
                          .Returns(generatedSequence);

            _mockGcdEstimator.Setup(g => g.CountGCD(1, 2)).Returns(1);
            _mockGcdEstimator.Setup(g => g.CountGCD(3, 4)).Returns(1);

            CesaroResultDto result = _service.EstimatePi(amount, null, null, null, null);

            _mockGenerator.Verify(g => g.GenerateRandomSequence(amount, null, null, null, null), Times.Once);
            Assert.Equal(2, result.pairs);
            Assert.Equal(2, result.coprime);
            Assert.Equal(Math.Sqrt(6.0), result.pi);
        }

        [Fact]
        public void EstimatePi_ShouldReturnNull_WhenGeneratorReturnsNull()
        {
            int amount = 10;
            _mockGenerator.Setup(g => g.GenerateRandomSequence(amount, null, null, null, null))
                          .Returns((List<long>)null);

            CesaroResultDto result = _service.EstimatePi(amount, null, null, null, null);

            Assert.Null(result);
        }
    }
}