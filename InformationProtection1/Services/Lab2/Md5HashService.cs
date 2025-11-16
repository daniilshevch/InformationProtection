using System.Text;

namespace InformationProtection1.Services.Lab2
{
    public class Md5HashService : IMd5HashService
    {
        private static readonly uint[] T = new uint[64];

        static Md5HashService()
        {
            for (int i = 0; i < 64; i++)
                T[i] = (uint)(Math.Abs(Math.Sin(i + 1)) * Math.Pow(2, 32));
        }

        private static uint LeftRotate(uint x, int n) =>
            (x << n) | (x >> (32 - n));

        private static readonly int[] S =
        {
            7,12,17,22, 7,12,17,22, 7,12,17,22, 7,12,17,22,
            5,9,14,20, 5,9,14,20, 5,9,14,20, 5,9,14,20,
            4,11,16,23, 4,11,16,23, 4,11,16,23, 4,11,16,23,
            6,10,15,21, 6,10,15,21, 6,10,15,21, 6,10,15,21
        };

        private static byte[] PadMessage(byte[] input)
        {
            ulong bitLength = (ulong)input.Length * 8;
            int paddingLength = (56 - (input.Length + 1) % 64 + 64) % 64;
            byte[] padded = new byte[input.Length + 1 + paddingLength + 8];

            Buffer.BlockCopy(input, 0, padded, 0, input.Length);
            padded[input.Length] = 0x80;

            for (int i = 0; i < 8; i++)
                padded[padded.Length - 8 + i] = (byte)((bitLength >> (8 * i)) & 0xFF);

            return padded;
        }

        public string ComputeMd5(string input)
        {
            byte[] message = Encoding.UTF8.GetBytes(input);
            return ComputeMd5Bytes(message);
        }

        public string ComputeMd5Bytes(byte[] message)
        {
            byte[] padded = PadMessage(message);

            uint A = 0x67452301;
            uint B = 0xEFCDAB89;
            uint C = 0x98BADCFE;
            uint D = 0x10325476;

            for (int i = 0; i < padded.Length; i += 64)
            {
                uint[] M = new uint[16];
                for (int j = 0; j < 16; j++)
                    M[j] = BitConverter.ToUInt32(padded, i + j * 4);

                uint a = A, b = B, c = C, d = D;

                for (int k = 0; k < 64; k++)
                {
                    uint F, g;
                    if (k < 16)
                    {
                        F = (b & c) | (~b & d);
                        g = (uint)k;
                    }
                    else if (k < 32)
                    {
                        F = (d & b) | (~d & c);
                        g = (uint)((5 * k + 1) % 16);
                    }
                    else if (k < 48)
                    {
                        F = b ^ c ^ d;
                        g = (uint)((3 * k + 5) % 16);
                    }
                    else
                    {
                        F = c ^ (b | ~d);
                        g = (uint)((7 * k) % 16);
                    }

                    uint temp = d;
                    d = c;
                    c = b;
                    b = b + LeftRotate(a + F + T[k] + M[g], S[k]);
                    a = temp;
                }

                A += a;
                B += b;
                C += c;
                D += d;
            }

            byte[] output = new byte[16];
            Array.Copy(BitConverter.GetBytes(A), 0, output, 0, 4);
            Array.Copy(BitConverter.GetBytes(B), 0, output, 4, 4);
            Array.Copy(BitConverter.GetBytes(C), 0, output, 8, 4);
            Array.Copy(BitConverter.GetBytes(D), 0, output, 12, 4);

            return BitConverter.ToString(output).Replace("-", "").ToUpper();
        }

        public async Task<string> ComputeFileMd5Async(IFormFile file)
        {
            using MemoryStream memory = new MemoryStream();
            await file.CopyToAsync(memory);
            return ComputeMd5Bytes(memory.ToArray());
        }

        public async Task<bool> VerifyFileIntegrityAsync(IFormFile file, string expectedMd5)
        {
            string computed = await ComputeFileMd5Async(file);
            return string.Equals(computed, expectedMd5, StringComparison.OrdinalIgnoreCase);
        }
    }
}

    

