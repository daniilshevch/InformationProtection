namespace InformationProtection1.Services.Lab1
{
    public class RandomSequenceGeneratorService
    {
        public List<long>? GenerateRandomSequence(int amount, long? _m = null, long? _a = null, long? _c = null, long? _X0 = null)
        {
            long m = _m ?? (long)Math.Pow(2, 31) - 3;
            long a = _a ?? (long)Math.Pow(2, 15);
            long c = _c ?? 46368;
            long X0 = _X0 ?? 37;
            List<long> X = new List<long>(amount);
            X.Add(X0);
            for(int i = 1; i < amount; i++)
            {
                long new_member = (a * X[i - 1] + c) % m;
                X.Add(new_member);
            }
            return X;
        }
        public List<long>? GeneratePartOfRandomSequence(int amount, int start, int end, long? _m, long? _a, long? _c, long? _X0)
        {
            List<long>? X = GenerateRandomSequence(amount, _m, _a, _c, _X0);
            if(X is null)
            {
                return null;
            }    
            List<long> partOfX = new List<long>(amount);
            for(int i = start; i < end; i++)
            {
                partOfX.Add(X[i]);
            }
            return partOfX;
        }
    }
}
