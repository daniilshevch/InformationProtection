using InformationProtection1.Dto.Lab5;
using InformationProtection1.Services.Lab5.Interfaces;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("lab5/[controller]")]
public class DSAController : ControllerBase
{
    private readonly IDsaService _dsaService;

    public DSAController(IDsaService dsaService)
    {
        _dsaService = dsaService;
    }

    [HttpPost("keys")]
    public IActionResult GenerateKeys()
    {
        var keys = _dsaService.GenerateKeys();
        return Ok(keys);
    }

    [HttpPost("sign/text")]
    public IActionResult SignText([FromBody] SignRequestDto request)
    {
        try
        {
            var signature = _dsaService.SignData(request.Data, request.PrivateKey);
            return Ok(new { SignatureHex = signature });
        }
        catch (Exception ex)
        {
            return BadRequest($"Error signing data: {ex.Message}");
        }
    }

    [HttpPost("sign/file")]
    public IActionResult SignFile(IFormFile file, [FromForm] string privateKey)
    {
        if (file == null || file.Length == 0)
            return BadRequest("File is empty");

        try
        {
            using (var stream = file.OpenReadStream())
            {
                var signature = _dsaService.SignFile(stream, privateKey);
                return Ok(new { SignatureHex = signature });
            }
        }
        catch (Exception ex)
        {
            return BadRequest($"Error signing file: {ex.Message}");
        }
    }

    [HttpPost("verify/text")]
    public IActionResult VerifyText([FromBody] VerifyRequestDto request)
    {
        try
        {
            bool isValid = _dsaService.VerifyData(request.Data, request.SignatureHex, request.PublicKey);
            return Ok(new { IsValid = isValid });
        }
        catch (Exception ex)
        {
            return BadRequest($"Error verifying data: {ex.Message}");
        }
    }

    [HttpPost("verify/file")]
    public IActionResult VerifyFile(IFormFile file, [FromForm] string signatureHex, [FromForm] string publicKey)
    {
        if (file == null || file.Length == 0)
            return BadRequest("File is empty");

        try
        {
            using (var stream = file.OpenReadStream())
            {
                bool isValid = _dsaService.VerifyFile(stream, signatureHex, publicKey);
                return Ok(new { IsValid = isValid });
            }
        }
        catch (Exception ex)
        {
            return BadRequest($"Error verifying file: {ex.Message}");
        }
    }
}