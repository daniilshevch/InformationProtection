using InformationProtection1.Services.Lab1;
using Microsoft.AspNetCore.Mvc;

namespace InformationProtection1.Controllers.Lab1
{
    public class PeriodCheckerController: ControllerBase
    {
        private readonly PeriodCheckerService periodCheckerService;
        public PeriodCheckerController(PeriodCheckerService periodCheckerService)
        {
            this.periodCheckerService = periodCheckerService;
        }
        [HttpGet("check-sequence-period")]
        public ActionResult<PeriodResultDto> CheckSequencePeriod(
            [FromQuery] long? m, 
            [FromQuery] long? a, 
            [FromQuery] long? c, 
            [FromQuery] long? X0)
        {
            PeriodResultDto periodResult = periodCheckerService.BrentAlgorithm(_m: m, _a: a, _c: c, _X0: X0);
            return Ok(periodResult);
        }
    }
}
