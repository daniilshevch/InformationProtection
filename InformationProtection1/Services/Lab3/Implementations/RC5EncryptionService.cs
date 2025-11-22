using System.Text;
using InformationProtection1.Services.Lab1.Interfaces;
using InformationProtection1.Services.Lab2.Interfaces;
using InformationProtection1.Services.Lab3.Interfaces;

namespace InformationProtection1.Services.Lab3.Implementations
{
    public class RC5EncryptionService : IRC5EncryptionService
    {
        private readonly ulong Pw = 0xB7E151628AED2A6B; 
        private readonly ulong Qw = 0x9E3779B97F4A7C15;

        private readonly IRandomSequenceGeneratorService randomGenerator;
        private readonly IMd5HashService md5Service;

        private const int BlockSize = 16;
        private const int WordSize = 8; 

        public RC5EncryptionService(IRandomSequenceGeneratorService randomGenerator, IMd5HashService md5Service)
        {
            this.randomGenerator = randomGenerator;
            this.md5Service = md5Service;
        }

        public static ulong RotateLeft(ulong x, int y) => (x << y) | (x >> (64 - y));
        public static ulong RotateRight(ulong x, int y) => (x >> y) | (x << (64 - y));

        private ulong[] GenerateSubKeys(byte[] key, int w, int r)
        {
            int u = w / 8; 
            int c = Math.Max(1, key.Length / u);
            ulong[] L = new ulong[c];
            for (int i = key.Length - 1; i >= 0; i--)
            {
                L[i / u] = (L[i / u] << 8) + key[i];
            }
            int t = 2 * r + 2;
            ulong[] S = new ulong[t];
            S[0] = Pw;
            for (int i = 1; i < t; i++)
            {
                S[i] = S[i - 1] + Qw;
            }
            ulong A = 0, B = 0;
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
            {
                return hash1.Take(8).ToArray();
            }
            if (keyBits == 128)
            {
                return hash1;
            }
            if (keyBits == 256)
            {
                string hash2 = md5Service.ComputeMd5Bytes(hash1);
                return Convert.FromHexString(hash2).Concat(hash1).ToArray();
            }
            return hash1.Take(8).ToArray(); 
        }

        public static byte[] ApplyPadding(byte[] data)
        {
            int pad = BlockSize - (data.Length % BlockSize);
            return data.Concat(Enumerable.Repeat((byte)pad, pad)).ToArray();
        }

        public static byte[] RemovePadding(byte[] data)
        {
            if (data.Length == 0)
            {
                return data;
            }
            int pad = data[^1];
            if (pad > BlockSize || pad == 0)
            {
                return data;
            }
            return data.Take(data.Length - pad).ToArray();
        }

        private byte[] EncryptBlock(byte[] block, ulong[] S, int r)
        {
            ulong A = BitConverter.ToUInt64(block, 0);
            ulong B = BitConverter.ToUInt64(block, 8);
            
            A += S[0];
            B += S[1];
            
            for (int i = 1; i <= r; i++)
            {
                A = RotateLeft(A ^ B, (int)B) + S[2 * i];
                B = RotateLeft(B ^ A, (int)A) + S[2 * i + 1];
            }
            
            byte[] result = new byte[BlockSize];
            Array.Copy(BitConverter.GetBytes(A), 0, result, 0, 8);
            Array.Copy(BitConverter.GetBytes(B), 0, result, 8, 8);
            return result;
        }

        private byte[] DecryptBlock(byte[] block, ulong[] S, int r)
        {
            ulong A = BitConverter.ToUInt64(block, 0);
            ulong B = BitConverter.ToUInt64(block, 8);
            
            for (int i = r; i >= 1; i--)
            {
                B = RotateRight(B - S[2 * i + 1], (int)A) ^ A;
                A = RotateRight(A - S[2 * i], (int)B) ^ B;
            }
            
            B -= S[1];
            A -= S[0];
            
            byte[] result = new byte[BlockSize];
            Array.Copy(BitConverter.GetBytes(A), 0, result, 0, 8);
            Array.Copy(BitConverter.GetBytes(B), 0, result, 8, 8);
            return result;
        }

        private byte[] GenerateIV()
        {
            List<long>? seq = randomGenerator.GenerateRandomSequence(BlockSize);
            byte[] iv = new byte[BlockSize];
            for (int i = 0; i < BlockSize; i++)
            {
                iv[i] = (byte)(seq![i % seq.Count] % 256);
            }
            return iv;
        }

        public async Task<(byte[] Encrypted, string Md5, string KeyBase64)> EncryptFileAsync(
            IFormFile file, string password, int keyBits = 64, int w = 64, int r = 20)
        {
            byte[] key = DeriveKeyFromPassword(password, keyBits);
            ulong[] S = GenerateSubKeys(key, w, r);
            byte[] iv = GenerateIV();

            using MemoryStream input = new();
            await file.CopyToAsync(input);
            byte[] data = ApplyPadding(input.ToArray());
            string md5 = md5Service.ComputeMd5Bytes(input.ToArray());

            using MemoryStream encrypted = new();
            
            byte[] ivEnc = EncryptBlock(iv, S, r);
            encrypted.Write(ivEnc);

            byte[] prev = ivEnc; 
            
            for (int i = 0; i < data.Length; i += BlockSize)
            {
                byte[] block = data.Skip(i).Take(BlockSize).ToArray();

                for (int j = 0; j < BlockSize; j++)
                {
                    block[j] ^= prev[j];
                }

                byte[] encBlock = EncryptBlock(block, S, r);
                encrypted.Write(encBlock);
                prev = encBlock;
            }

            byte[] encryptedBytes = encrypted.ToArray();
            
            string outputFilePath = Path.Combine(Directory.GetCurrentDirectory(), "encrypted_result_v24.bin");
            await File.WriteAllBytesAsync(outputFilePath, encryptedBytes);

            return (encryptedBytes, md5, Convert.ToBase64String(key));
        }

        public async Task<byte[]> DecryptFileAsync(IFormFile file, string password, int keyBits = 64, int w = 64, int r = 20)
        {
            byte[] key = DeriveKeyFromPassword(password, keyBits);
            ulong[] S = GenerateSubKeys(key, w, r);

            using MemoryStream input = new();
            await file.CopyToAsync(input);
            byte[] cipher = input.ToArray();

            if (cipher.Length == 0 || cipher.Length % BlockSize != 0)
            {
                throw new Exception($"Файл пошкоджено. Розмір має бути кратним {BlockSize}.");
            }

            byte[] ivEnc = cipher.Take(BlockSize).ToArray();
            byte[] iv = DecryptBlock(ivEnc, S, r); 

            byte[] prev = ivEnc; 
            
            using MemoryStream decrypted = new();

            for (int i = BlockSize; i < cipher.Length; i += BlockSize)
            {
                byte[] block = cipher.Skip(i).Take(BlockSize).ToArray();
                
                byte[] decBlock = DecryptBlock(block, S, r);

                for (int j = 0; j < BlockSize; j++)
                {
                    decBlock[j] ^= prev[j];
                }

                decrypted.Write(decBlock);
                prev = block;
            }

            return RemovePadding(decrypted.ToArray());
        }


        public string EncryptText(string plainText, string password, int keyBits = 64, int w = 64, int r = 20)
        {
            byte[] key = DeriveKeyFromPassword(password, keyBits);
            ulong[] S = GenerateSubKeys(key, w, r);
            byte[] iv = GenerateIV(); 

            byte[] data = ApplyPadding(Encoding.UTF8.GetBytes(plainText));

            using MemoryStream encrypted = new();

            byte[] ivEnc = EncryptBlock(iv, S, r);
            encrypted.Write(ivEnc);

            byte[] prev = ivEnc; 

            for (int i = 0; i < data.Length; i += BlockSize)
            {
                byte[] block = data.Skip(i).Take(BlockSize).ToArray();

                for (int j = 0; j < BlockSize; j++)
                {
                    block[j] ^= prev[j];
                }

                byte[] encBlock = EncryptBlock(block, S, r);
                encrypted.Write(encBlock);

                prev = encBlock; 
            }

            string md5 = md5Service.ComputeMd5(plainText);
            return $"{Convert.ToBase64String(encrypted.ToArray())}|{Convert.ToBase64String(key)}|{md5}";
        }

        public string DecryptText(string combined, int w = 64, int r = 20)
        {
            var parts = combined.Split('|');
            if (parts.Length < 2)
            {
                throw new ArgumentException("Невірний формат зашифрованого рядка.");
            }

            byte[] cipher = Convert.FromBase64String(parts[0]);
            byte[] key = Convert.FromBase64String(parts[1]);

            ulong[] S = GenerateSubKeys(key, w, r);

            if (cipher.Length == 0 || cipher.Length % BlockSize != 0)
            {
                throw new ArgumentException($"Довжина шифротексту ({cipher.Length}) не кратна блоку ({BlockSize}).");
            }

            byte[] ivEnc = cipher.Take(BlockSize).ToArray();
            byte[] iv = DecryptBlock(ivEnc, S, r); 

            byte[] prev = ivEnc; 

            using MemoryStream decrypted = new();

            for (int i = BlockSize; i < cipher.Length; i += BlockSize)
            {
                byte[] block = cipher.Skip(i).Take(BlockSize).ToArray();

                byte[] decBlock = DecryptBlock(block, S, r);

                for (int j = 0; j < BlockSize; j++)
                {
                    decBlock[j] ^= prev[j];
                }

                decrypted.Write(decBlock);
                prev = block;
            }

            byte[] plainBytes = RemovePadding(decrypted.ToArray());
            return Encoding.UTF8.GetString(plainBytes);
        }

        public static string DetectFileExtension(byte[] fileBytes)
        {
             if (fileBytes.Length < 8) return ".bin";
             if (fileBytes[0] == 0x89 && fileBytes[1] == 0x50 && fileBytes[2] == 0x4E && fileBytes[3] == 0x47) return ".png";
             if (fileBytes[0] == 0xFF && fileBytes[1] == 0xD8) return ".jpg";
             if (fileBytes[0] == 0x25 && fileBytes[1] == 0x50 && fileBytes[2] == 0x44 && fileBytes[3] == 0x46) return ".pdf";
             if (fileBytes[0] == 0x50 && fileBytes[1] == 0x4B && fileBytes[2] == 0x03 && fileBytes[3] == 0x04) return ".docx";
             if (fileBytes[0] == 0x49 && fileBytes[1] == 0x44 && fileBytes[2] == 0x33) return ".mp3";
             if (fileBytes.Length > 8 && fileBytes[4] == 0x66 && fileBytes[5] == 0x74 && fileBytes[6] == 0x79 && fileBytes[7] == 0x70) return ".mp4";
             return ".bin";
        }
    }
}