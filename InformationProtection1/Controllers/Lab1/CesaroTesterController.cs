using InformationProtection1.Dto.Lab1;
using InformationProtection1.Services.Lab1;
using Microsoft.AspNetCore.Mvc;

namespace InformationProtection1.Controllers.Lab1
{
    public class CesaroTesterController: ControllerBase
    {
        private readonly CesaroTesterService cesaroTesterService;
        public CesaroTesterController(CesaroTesterService cesaroTesterService)
        {
            this.cesaroTesterService = cesaroTesterService;
        }
        [HttpGet("estimate-pi/{amount}")]
        public ActionResult<CesaroResultDto> EstimatePi(
            [FromRoute] int amount, 
            [FromQuery] long? m, 
            [FromQuery] long? a, 
            [FromQuery] long? c, 
            [FromQuery] long? X0)
        {
            CesaroResultDto? cesaroResult = cesaroTesterService.EstimatePi(amount, m: m, a: a, c: c, X0: X0);
            if(cesaroResult is null)
            {
                return BadRequest("Size of sequence for testing is not valid");
            }    
            return Ok(cesaroResult);
        }
    }
}
