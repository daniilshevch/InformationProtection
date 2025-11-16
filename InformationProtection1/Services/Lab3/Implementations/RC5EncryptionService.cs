using System.Text;
using InformationProtection1.Services.Lab1.Interfaces;
using InformationProtection1.Services.Lab2.Interfaces;
using InformationProtection1.Services.Lab3.Interfaces;

namespace InformationProtection1.Services.Lab3.Implementations
{
    public class RC5EncryptionService : IRC5EncryptionService
    {
        private readonly uint Pw = 0xB7E15163;
        private readonly uint Qw = 0x9E3779B9;
        private readonly IRandomSequenceGeneratorService randomGenerator;
        private readonly IMd5HashService md5Service;

        public static string DetectFileExtension(byte[] fileBytes)
        {
            if (fileBytes.Length < 8) return ".bin";

            if (fileBytes[0] == 0x89 && fileBytes[1] == 0x50 && fileBytes[2] == 0x4E && fileBytes[3] == 0x47)
                return ".png";

            if (fileBytes[0] == 0xFF && fileBytes[1] == 0xD8)
                return ".jpg";

            if (fileBytes[0] == 0x25 && fileBytes[1] == 0x50 && fileBytes[2] == 0x44 && fileBytes[3] == 0x46)
                return ".pdf";

            if (fileBytes[0] == 0x50 && fileBytes[1] == 0x4B && fileBytes[2] == 0x03 && fileBytes[3] == 0x04)
                return ".docx";

            if (fileBytes[0] == 0x49 && fileBytes[1] == 0x44 && fileBytes[2] == 0x33)
                return ".mp3";
            if (fileBytes[4] == 0x66 && fileBytes[5] == 0x74 && fileBytes[6] == 0x79 && fileBytes[7] == 0x70)
                return ".mp4";

            return ".bin";
        }
        public RC5EncryptionService(IRandomSequenceGeneratorService randomGenerator, IMd5HashService md5Service)
        {
            this.randomGenerator = randomGenerator;
            this.md5Service = md5Service;
        }

        public static uint RotateLeft(uint x, int y) => x << y | x >> 32 - y;
        public static uint RotateRight(uint x, int y) => x >> y | x << 32 - y;

        private uint[] GenerateSubKeys(byte[] key, int w, int r)
        {
            int u = w / 8;
            int c = Math.Max(1, key.Length / u);
            uint[] L = new uint[c];
            for (int i = key.Length - 1; i >= 0; i--)
                L[i / u] = (L[i / u] << 8) + key[i];

            int t = 2 * r + 2;
            uint[] S = new uint[t];
            S[0] = Pw;
            for (int i = 1; i < t; i++)
                S[i] = S[i - 1] + Qw;

            uint A = 0, B = 0;
            int iter = 3 * Math.Max(t, c);
            int ii = 0, jj = 0;
            for (int k = 0; k < iter; k++)
            {
                A = S[ii] = RotateLeft(S[ii] + A + B, 3);
                B = L[jj] = RotateLeft(L[jj] + A + B, (int)(A + B));
                ii = (ii + 1) % t;
                jj = (jj + 1) % c;
            }
            return S;
        }

        public byte[] DeriveKeyFromPassword(string password, int keyBits)
        {
            byte[] hash1 = Convert.FromHexString(md5Service.ComputeMd5(password));

            if (keyBits == 64)
                return hash1.Take(8).ToArray();

            if (keyBits == 128)
                return hash1;

            if (keyBits == 256)
            {
                string hash2 = md5Service.ComputeMd5Bytes(hash1);
                byte[] concat = Convert.FromHexString(hash2).Concat(hash1).ToArray();
                return concat;
            }

            throw new ArgumentException("Unsupported key size (64, 128, or 256 bits).");
        }

        public static byte[] ApplyPadding(byte[] data)
        {
            int pad = 8 - data.Length % 8;
            return data.Concat(Enumerable.Repeat((byte)pad, pad)).ToArray();
        }

        public static byte[] RemovePadding(byte[] data)
        {
            int pad = data[^1];
            return data.Take(data.Length - pad).ToArray();
        }

        private byte[] EncryptBlock(byte[] block, uint[] S, int r)
        {
            uint A = BitConverter.ToUInt32(block, 0);
            uint B = BitConverter.ToUInt32(block, 4);
            A += S[0];
            B += S[1];
            for (int i = 1; i <= r; i++)
            {
                A = RotateLeft(A ^ B, (int)B) + S[2 * i];
                B = RotateLeft(B ^ A, (int)A) + S[2 * i + 1];
            }
            byte[] result = new byte[8];
            Array.Copy(BitConverter.GetBytes(A), 0, result, 0, 4);
            Array.Copy(BitConverter.GetBytes(B), 0, result, 4, 4);
            return result;
        }

        private byte[] DecryptBlock(byte[] block, uint[] S, int r)
        {
            uint A = BitConverter.ToUInt32(block, 0);
            uint B = BitConverter.ToUInt32(block, 4);
            for (int i = r; i >= 1; i--)
            {
                B = RotateRight(B - S[2 * i + 1], (int)A) ^ A;
                A = RotateRight(A - S[2 * i], (int)B) ^ B;
            }
            A -= S[0];
            B -= S[1];
            byte[] result = new byte[8];
            Array.Copy(BitConverter.GetBytes(A), 0, result, 0, 4);
            Array.Copy(BitConverter.GetBytes(B), 0, result, 4, 4);
            return result;
        }

        private byte[] GenerateIV()
        {
            List<long> seq = randomGenerator.GenerateRandomSequence(2);
            byte[] iv = new byte[8];
            for (int i = 0; i < 8; i++)
                iv[i] = (byte)(seq[i % seq.Count] % 256);
            return iv;
        }

        public string EncryptText(string plainText, string password, int keyBits, int w = 32, int r = 12)
        {
            byte[] key = DeriveKeyFromPassword(password, keyBits);
            uint[] S = GenerateSubKeys(key, w, r);
            byte[] iv = GenerateIV();

            byte[] data = ApplyPadding(Encoding.UTF8.GetBytes(plainText));
            MemoryStream encrypted = new();

            byte[] ivEnc = EncryptBlock(iv, S, r);
            encrypted.Write(ivEnc);

            byte[] prev = iv;
            for (int i = 0; i < data.Length; i += 8)
            {
                byte[] block = data.Skip(i).Take(8).ToArray();
                for (int j = 0; j < 8; j++)
                    block[j] ^= prev[j];

                byte[] encBlock = EncryptBlock(block, S, r);
                encrypted.Write(encBlock);
                prev = encBlock;
            }

            string md5 = md5Service.ComputeMd5(plainText);
            return $"{Convert.ToBase64String(encrypted.ToArray())}|{Convert.ToBase64String(key)}|{md5}";
        }

        public string DecryptText(string combined, int w = 32, int r = 12)
        {
            var parts = combined.Split('|');
            byte[] cipher = Convert.FromBase64String(parts[0]);
            byte[] key = Convert.FromBase64String(parts[1]);
            uint[] S = GenerateSubKeys(key, w, r);

            byte[] ivEnc = cipher.Take(8).ToArray();
            byte[] iv = DecryptBlock(ivEnc, S, r);

            byte[] prev = iv;
            MemoryStream decrypted = new();

            for (int i = 8; i < cipher.Length; i += 8)
            {
                byte[] block = cipher.Skip(i).Take(8).ToArray();
                byte[] decBlock = DecryptBlock(block, S, r);
                for (int j = 0; j < 8; j++)
                    decBlock[j] ^= prev[j];

                decrypted.Write(decBlock);
                prev = block;
            }

            return Encoding.UTF8.GetString(RemovePadding(decrypted.ToArray()));
        }

        public async Task<(byte[] Encrypted, string Md5, string KeyBase64)> EncryptFileAsync(
            IFormFile file, string password, int keyBits, int w = 32, int r = 12)
        {
            byte[] key = DeriveKeyFromPassword(password, keyBits);
            uint[] S = GenerateSubKeys(key, w, r);
            byte[] iv = GenerateIV();

            using MemoryStream input = new();
            await file.CopyToAsync(input);
            byte[] data = ApplyPadding(input.ToArray());
            string md5 = md5Service.ComputeMd5Bytes(input.ToArray());

            MemoryStream encrypted = new();
            byte[] ivEnc = EncryptBlock(iv, S, r);
            encrypted.Write(ivEnc);

            byte[] prev = iv;
            for (int i = 0; i < data.Length; i += 8)
            {
                byte[] block = data.Skip(i).Take(8).ToArray();
                for (int j = 0; j < 8; j++)
                    block[j] ^= prev[j];

                byte[] encBlock = EncryptBlock(block, S, r);
                encrypted.Write(encBlock);
                prev = encBlock;
            }
            byte[] encryptedBytes = encrypted.ToArray();

            string outputFilePath = Path.Combine(Directory.GetCurrentDirectory(), "encrypted_result.bin");
            await File.WriteAllBytesAsync(outputFilePath, encryptedBytes);

            Console.WriteLine($"Зашифрований файл збережено: {outputFilePath}");

            return (encrypted.ToArray(), md5, Convert.ToBase64String(key));
        }

        public async Task<byte[]> DecryptFileAsync(IFormFile file, string password, int keyBits, int w = 32, int r = 12)
        {
            byte[] key = DeriveKeyFromPassword(password, keyBits);
            uint[] S = GenerateSubKeys(key, w, r);

            using MemoryStream input = new();
            await file.CopyToAsync(input);
            byte[] cipher = input.ToArray();

            byte[] ivEnc = cipher.Take(8).ToArray();
            byte[] iv = DecryptBlock(ivEnc, S, r);

            byte[] prev = iv;
            MemoryStream decrypted = new();


            for (int i = 8; i < cipher.Length; i += 8)
            {
                byte[] block = cipher.Skip(i).Take(8).ToArray();
                byte[] decBlock = DecryptBlock(block, S, r);
                for (int j = 0; j < 8; j++)
                    decBlock[j] ^= prev[j];
                decrypted.Write(decBlock);
                prev = block;
            }

            return RemovePadding(decrypted.ToArray());
        }
    }
}
