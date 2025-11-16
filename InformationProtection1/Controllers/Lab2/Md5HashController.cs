using InformationProtection1.Dto.General;
using InformationProtection1.Dto.Lab2;
using InformationProtection1.Services.Lab2.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace InformationProtection1.Controllers.Lab2
{
    [ApiController]
    [Route("lab2/[controller]")]
    public class Md5HashController : ControllerBase
    {
        private readonly IMd5HashService md5HashService;

        public Md5HashController(IMd5HashService md5HashService)
        {
            this.md5HashService = md5HashService;
        }

        [HttpPost("hash-text")]
        public ActionResult<HashResponseDto> ComputeTextHash([FromBody] HashRequestDto dto)
        {
            string hash = md5HashService.ComputeMd5(dto.InputText);
            return Ok(new HashResponseDto
            {
                Input = dto.InputText,
                Md5Hash = hash
            });
        }

        [HttpPost("hash-file")]
        public async Task<ActionResult<HashResponseDto>> ComputeFileHash([FromForm] FileUploadDto dto)
        {
            Stopwatch sw = Stopwatch.StartNew();
            string hash = await md5HashService.ComputeFileMd5Async(dto.File);
            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds / 1000);
            return Ok(new HashResponseDto
            {
                Input = dto.File.FileName,
                Md5Hash = hash,
                GenerationTime = TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds),
            });
        }

        [HttpPost("verify-file-integrity")]
        public async Task<IActionResult> VerifyFile([FromForm] FileIntegrityCheckDto dto)
        {
            bool ok = await md5HashService.VerifyFileIntegrityAsync(dto.File, dto.ExpectedMd5Hash);
            return Ok(new
            {
                File = dto.File.FileName,
                Expected = dto.ExpectedMd5Hash,
                Result = ok ? "Файл цілісний" : "Хеш не співпадає"
            });
        }

        [HttpGet("test")]
        public IActionResult TestVectors()
        {
            Dictionary<string, string> tests = new Dictionary<string, string>
            {
                { "", "D41D8CD98F00B204E9800998ECF8427E" },
                { "a", "0CC175B9C0F1B6A831C399E269772661" },
                { "abc", "900150983CD24FB0D6963F7D28E17F72" },
                { "message digest", "F96B697D7CB7938D525A2F31AAF161D0" },
                { "abcdefghijklmnopqrstuvwxyz", "C3FCD3D76192E4007DFB496CCA67E13B" },
                {"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789", "D174AB98D277D9F5A5611C2C9F419D9F" },
                {"12345678901234567890123456789012345678901234567890123456789012345678901234567890", "57EDF4A22BE3C955AC49DA2E2107B67A" }
            };

            var result = tests.Select(t => new
            {
                Input = t.Key,
                Expected = t.Value,
                Actual = md5HashService.ComputeMd5(t.Key)
            });

            return Ok(result);
        }
    }
}

