namespace InformationProtection1.Services.Lab1
{
    public class FileSaverService
    {
        public async Task<string> SaveAsync(List<long> sequence, string root, 
            long? _m = null, long? _a = null, long? _c = null, long? _X0 = null)
        {
            long m = _m ?? (long)Math.Pow(2, 31) - 3;
            long a = _a ?? (long)Math.Pow(2, 15);
            long c = _c ?? 46368;
            long X0 = _X0 ?? 37;
            string directory = Path.Combine(root, "results");
            Directory.CreateDirectory(directory);
            string path = Path.Combine(directory, $"seq_{DateTime.Now}.txt");
            List<string> lines = new List<string>
            {
                $"m = {m}",
                $"a = {a}",
                $"c = {c}",
                $"X0 = {X0}",
                "------------------", 
            };
            lines.AddRange(sequence.Select(num => num.ToString()));
            await File.WriteAllLinesAsync(path, lines);
            return path;
        }
    }
}
