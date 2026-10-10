using LoanApplication.Application.Interfcae;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LoanApplication.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SanctionLetterController : ControllerBase
    {
        private readonly ISanctionLetterService service;

        public SanctionLetterController(
            ISanctionLetterService service)
        {
            this.service = service;
        }

        [HttpGet("approved-loans")]
        public async Task<IActionResult> GetApprovedLoans()
        {
            var data = await service.GetApprovedLoansAsync();

            return Ok(data);
        }
    }
}

