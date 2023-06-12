using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineShop2.Api.Services.ReportsServices;

namespace OnlineShop2.Api.Controllers.ReportsControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShiftReportController : ControllerBase
    {
        private readonly ShiftReportService _service;
        public ShiftReportController(ShiftReportService service)
        {
            _service = service;
        }

        [HttpGet("/api/{shopId}/reports/shifts")]
        public async Task<IActionResult> GetShifts(int shopId, DateTime with, DateTime by) =>
            Ok(await _service.GetShifts(shopId, with, by));

        [HttpGet("/api/{shopId}/reports/shiftsummary/{shiftId}")]
        public async Task<IActionResult> GetSummary(int shopId, int shiftId) =>
            Ok(await _service.GetSummary(shiftId));
    }
}
