namespace InformationProtection1.Services.Lab1.Interfaces
{
    public interface IRandomSequenceGeneratorService
    {
        List<long>? GenerateRandomSequence(int amount, long? _m = null, long? _a = null, long? _c = null, long? _X0 = null);
        List<long>? GeneratePartOfRandomSequence(int amount, int start, int end, long? _m, long? _a, long? _c, long? _X0);
    }
}