using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop2.Api.Services.ReportsServices;
using OnlineShop2.Database;
using OnlineShop2.Database.Models;

namespace OnlineShop2.Api.Controllers.ReportsControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShiftReportController : ControllerBase
    {
        private readonly ShiftReportService _service;
        private readonly OnlineShopContext _context;
        public ShiftReportController(ShiftReportService service, OnlineShopContext context)
        {
            _service = service;
            _context = context;
        }

        [HttpGet("/api/{shopId}/reports/shifts")]
        public async Task<IActionResult> GetShifts(int shopId, DateTime with, DateTime by) =>
            Ok(await _service.GetShifts(shopId, with, by));

        [HttpGet("/api/{shopId}/reports/shiftsummary/{shiftId}")]
        public async Task<IActionResult> GetSummary(int shopId, int shiftId) =>
            Ok(await _service.GetSummary(shiftId));

        [HttpGet("/api/{shopId}/reports/shifts/{shiftId}")]
        public async Task<IActionResult> GetOne(int shopId, int shiftId) =>
            Ok(await _service.GetOne(shiftId));
    }
}
