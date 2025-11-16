using InformationProtection1.Dto.Lab1;
namespace InformationProtection1.Services.Lab1.Interfaces
{
    public interface ICesaroTesterService
    {
        CesaroResultDto? EstimatePi(int amount, long? m = null, long? a = null, long? c = null, long? X0 = null);
        CesaroResultDto EstimatePiWithSequence(List<long> sequence);
    }
}