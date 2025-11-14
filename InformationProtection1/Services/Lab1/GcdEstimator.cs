namespace InformationProtection1.Services.Lab1
{
    public class GcdEstimator: IGcdEstimator
    {
        public long CountGCD(long a, long b)
        {
            a = Math.Abs(a);
            b = Math.Abs(b);
            while (b != 0)
            {
                long r = a % b;
                a = b;
                b = r;
            }
            return a;
        }
    }
}
