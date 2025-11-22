using InformationProtection1.Services.Lab1.Implementations;
using InformationProtection1.Services.Lab1.Interfaces;
using InformationProtection1.Dto.Lab1;
using Moq;

namespace InformationProtection1.Tests.Lab1.Services
{
    public class CesaroTesterServiceTests
    {
        private readonly Mock<IGcdEstimator> mockGcdEstimator = new();
        private readonly Mock<IRandomSequenceGeneratorService> mockGeneratorService = new();
        private readonly CesaroTesterService cesaroTesterService;

        public CesaroTesterServiceTests()
        {
            cesaroTesterService = new CesaroTesterService(mockGcdEstimator.Object, mockGeneratorService.Object);
        }

        [Fact]
        public void EstimatePiWithSequence_ShouldHandleEmptySequence()
        {
            var result = cesaroTesterService.EstimatePiWithSequence(new List<long>());

            Assert.True(double.IsNaN(result.pi));
            Assert.True(double.IsNaN(result.probability));
            Assert.Equal(0, result.pairs);
            Assert.Equal(0, result.coprime);
        }

        [Fact]
        public void EstimatePiWithSequence_ShouldHandleSingleElement()
        {
            var result = cesaroTesterService.EstimatePiWithSequence(new List<long> { 10 });

            Assert.True(double.IsNaN(result.pi));
            Assert.True(double.IsNaN(result.probability));
            Assert.Equal(0, result.pairs);
            Assert.Equal(0, result.coprime);
        }

        [Fact]
        public void EstimatePiWithSequence_ShouldCalculateCorrectly()
        {
            var sequence = new List<long> { 1, 2, 3, 4, 6, 9 };

            mockGcdEstimator.Setup(m => m.CountGCD(1, 2)).Returns(1);
            mockGcdEstimator.Setup(m => m.CountGCD(3, 4)).Returns(1);
            mockGcdEstimator.Setup(m => m.CountGCD(6, 9)).Returns(3);

            var result = cesaroTesterService.EstimatePiWithSequence(sequence);

            double expectedProbability = 2.0 / 3.0;
            double expectedPi = Math.Sqrt(6.0 / expectedProbability); 

            Assert.Equal(3.0, result.pi, 5); 
            Assert.Equal(expectedProbability, result.probability, 5);
            Assert.Equal(3, result.pairs);
            Assert.Equal(2, result.coprime);
        }

        [Fact]
        public void EstimatePi_ShouldReturnNull_WhenGeneratorFails()
        {
            int amount = 10;
            mockGeneratorService.Setup(m => m.GenerateRandomSequence(
                amount, It.IsAny<long?>(), It.IsAny<long?>(), It.IsAny<long?>(), It.IsAny<long?>()))
                .Returns((List<long>?)null);

            var result = cesaroTesterService.EstimatePi(amount);

            Assert.Null(result);
        }
    }
}