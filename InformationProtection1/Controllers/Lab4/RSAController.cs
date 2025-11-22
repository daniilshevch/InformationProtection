using InformationProtection1.Dto.Lab4;
using InformationProtection1.Services.Lab4.Implementations;
using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using InformationProtection1.Services.Lab4.Interfaces;

namespace InformationProtection1.Controllers.Lab4
{
    [ApiController]
    [Route("lab4/[controller]")]
    public class RSAController : ControllerBase
    {
        private readonly IRSAService rsaService;

        public RSAController(IRSAService rsaService)
        {
            this.rsaService = rsaService;
        }
        [HttpPost("generate-keys")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        public IActionResult GenerateKeysAndDownload()
        {
            var (publicKeyPem, privateKeyPem) = rsaService.GenerateKeys();

            using var memoryStream = new MemoryStream();
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                var pubEntry = archive.CreateEntry("public_key.pem");
                using (var entryStream = pubEntry.Open())
                using (var streamWriter = new StreamWriter(entryStream, Encoding.UTF8))
                {
                    streamWriter.Write(publicKeyPem);
                }

                var privEntry = archive.CreateEntry("private_key.pem");
                using (var entryStream = privEntry.Open())
                using (var streamWriter = new StreamWriter(entryStream, Encoding.UTF8))
                {
                    streamWriter.Write(privateKeyPem);
                }
            }

            memoryStream.Seek(0, SeekOrigin.Begin);
            return File(memoryStream.ToArray(), "application/zip", "rsa_keys.zip");
        }

        [HttpPost("encrypt-file")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> EncryptFile([FromForm] RSAEncryptFileRequestDto request)
        {
            if (request.InputFile == null || request.PublicKeyFile == null)
                return BadRequest("Потрібно надати і файл, і ключ.");

            string publicKeyPem;
            using (var reader = new StreamReader(request.PublicKeyFile.OpenReadStream()))
            {
                publicKeyPem = await reader.ReadToEndAsync();
            }

            var outputStream = new MemoryStream();
            long encryptionTimeMs;

            try
            {
                encryptionTimeMs = await rsaService.EncryptStreamAsync(request.InputFile.OpenReadStream(), publicKeyPem, outputStream);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest($"Помилка: Некоректний формат публічного ключа. Перевірте, чи це файл PEM. ({ex.Message})");
            }
            catch (CryptographicException ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest($"Помилка шифрування: Некоректний публічний ключ. ({ex.Message})");
            }

            outputStream.Seek(0, SeekOrigin.Begin);

            Response.Headers.Append("X-Encryption-Time-Ms", encryptionTimeMs.ToString());

            string filename = $"encrypted_{request.InputFile.FileName}.dat";

            return File(outputStream.ToArray(), "application/octet-stream", filename);
        }

        [HttpPost("decrypt-file")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> DecryptFile([FromForm] RSADecryptFileRequestDto request)
        {
            if (request.EncryptedFile == null || request.PrivateKeyFile == null)
                return BadRequest("Потрібно надати і зашифрований файл, і ключ.");

            string privateKeyPem;
            using (var reader = new StreamReader(request.PrivateKeyFile.OpenReadStream()))
            {
                privateKeyPem = await reader.ReadToEndAsync();
            }

            var outputStream = new MemoryStream();

            try
            {
                await rsaService.DecryptStreamAsync(request.EncryptedFile.OpenReadStream(), privateKeyPem, outputStream);
            }
            catch (ArgumentException ex)
            {
                return BadRequest($"Помилка: Некоректний формат приватного ключа. Перевірте, чи це файл PEM. ({ex.Message})");
            }
            catch (CryptographicException ex)
            {
                return BadRequest($"Помилка дешифрування. Можливо, використовується невідповідний приватний ключ. ({ex.Message})");
            }
            catch (EndOfStreamException ex)
            {
                return BadRequest($"Помилка читання файлу, можливо, він пошкоджений. ({ex.Message})");
            }
            outputStream.Seek(0, SeekOrigin.Begin);
            string originalFileName = Path.GetFileNameWithoutExtension(request.EncryptedFile.FileName);
            return File(outputStream.ToArray(), "application/octet-stream", $"decrypted_{originalFileName}");
        }
    }
}