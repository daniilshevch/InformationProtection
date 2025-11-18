using Microsoft.AspNetCore.Mvc;
using InformationProtection1.Dto.Lab3;
using InformationProtection1.Services.Lab3.Implementations;
using InformationProtection1.Services.Lab3.Interfaces;
namespace InformationProtection1.Controllers.Lab3
{
    [ApiController]
    [Route("lab3/[controller]")]
    public class Rc5EncryptionController : ControllerBase
    {
        private readonly IRC5EncryptionService rc5Service;

        public Rc5EncryptionController(IRC5EncryptionService rc5Service)
        {
            this.rc5Service = rc5Service;
        }

        [HttpPost("encrypt-text")]
        public ActionResult<RC5EncryptResponseDto> EncryptText([FromBody] RC5EncryptRequestDto dto)
        {
            string encrypted = rc5Service.EncryptText(dto.PlainText, dto.Password, dto.KeyBits);
            string[] parts = encrypted.Split('|');
            return Ok(new RC5EncryptResponseDto
            {
                EncryptedText = parts[0],
                KeyBase64 = parts[1],
                Md5Hash = parts[2]
            });
        }

        [HttpPost("decrypt-text")]
        public ActionResult<string> DecryptText([FromBody] RC5EncryptResponseDto dto)
        {
            string combined = $"{dto.EncryptedText}|{dto.KeyBase64}|{dto.Md5Hash}";
            string decrypted = rc5Service.DecryptText(combined);
            return Ok(new { Decrypted = decrypted });
        }

        [HttpPost("encrypt-file")]
        public async Task<ActionResult<RC5FileEncryptResponseDto>> EncryptFile([FromForm] RC5FileEncryptRequestDto dto)
        {
            var (encrypted, md5, key) = await rc5Service.EncryptFileAsync(dto.File, dto.Password, dto.KeyBits);
            return Ok(new RC5FileEncryptResponseDto
            {
                FileName = dto.File.FileName,
                EncryptedData = encrypted,
                Md5Hash = md5,
                KeyBase64 = key
            });
        }
        [HttpPost("decrypt-file")]
        public async Task<IActionResult> DecryptFile([FromForm] RC5FileEncryptRequestDto dto)
        {
            byte[] decryptedBytes = await rc5Service.DecryptFileAsync(dto.File, dto.Password, dto.KeyBits);
            //byte[] decryptedBytes = RC5EncryptionService.RemovePadding(decrypted);

            string ext = RC5EncryptionService.DetectFileExtension(decryptedBytes);
            string originalName = Path.GetFileNameWithoutExtension(dto.File.FileName);
            string outputFileName = $"decrypted_{originalName}{ext}";

            var result = new FileContentResult(decryptedBytes, "application/octet-stream")
            {
                FileDownloadName = outputFileName
            };

            result.FileDownloadName = outputFileName;
            Response.Headers["Content-Disposition"] = $"attachment; filename=\"{outputFileName}\"";

            return result;
        }

    }
}
