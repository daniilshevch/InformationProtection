using InformationProtection1.Services.Lab1;
using Microsoft.AspNetCore.Mvc;

namespace InformationProtection1.Controllers.Lab1
{
    public class RandomSequenceGeneratorController: ControllerBase
    {
        private readonly RandomSequenceGeneratorService randomSequenceGeneratorService;
        public RandomSequenceGeneratorController(RandomSequenceGeneratorService randomSequenceGeneratorService)
        {
            this.randomSequenceGeneratorService = randomSequenceGeneratorService;
        }
        [HttpGet("/get-random-sequence/{amount}")]
        public ActionResult<List<long>> GenerateRandomSequenceWithDefinedParameters(
            [FromRoute] int amount,
            [FromQuery] long? m,
            [FromQuery] long? a,
            [FromQuery] long? c,
            [FromQuery] long? X0)
        {

            List<long>? sequence = randomSequenceGeneratorService.GenerateRandomSequence(
                amount: amount, _m: m, _a: a, _c: c, _X0: X0);
            if(sequence is null)
            {
                return BadRequest("Sequence size is not valid");
            }
            return Ok(sequence);

        }
        [HttpGet("/get-random-sequence-with-pagination/{amount}/{start}/{end}")]
        public ActionResult<List<long>> GenerateRandomSequenceWithDefinedParameters(
            [FromRoute] int amount, 
            [FromRoute] int start,
            [FromRoute] int end,
            [FromQuery] long? m,
            [FromQuery] long? a,
            [FromQuery] long? c,
            [FromQuery] long? X0
            )
        {
            List<long>? sequence = randomSequenceGeneratorService.GeneratePartOfRandomSequence(
                amount: amount, 
                start: start,
                end: end,
                _m: m, _a: a, _c: c, _X0: X0);
            if (sequence is null)
            {
                return BadRequest("Sequence size is not valid");
            }
            return Ok(sequence);

        }
    }
}
