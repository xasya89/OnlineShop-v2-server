using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using OnlineShop2.Api.Services;

namespace OnlineShop2.Api.Controllers.MoneyReport
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoneyReportController : ControllerBase
    {
        private readonly MoneyReportService _reportService;
        public MoneyReportController(MoneyReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("/api/{shopId}/moneyreport")]
        public async Task<IActionResult> Get(int shopId, DateTime with, DateTime by) =>
            Ok(await _reportService.Get(shopId, with, by));
    }
}
