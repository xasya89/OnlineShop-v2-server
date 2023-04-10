using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineShop2.Api.Models.Arrival;
using OnlineShop2.Api.Services;

namespace OnlineShop2.Api.Controllers.Arrivals
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ArrivalController : ControllerBase
    {
        public readonly ArrivalService _service;

        public ArrivalController(ArrivalService service) => _service = service;

        [HttpGet("/api/{shopId}/arrivals")]
        public async Task<ArrivalSummaryResponseModel[]> Get(int shopId, [FromQuery] int page=0, [FromQuery] int count=50, [FromQuery] int? supplierId=null) =>
            await _service.GetArrivals(page, count, shopId, supplierId);

        [HttpGet("/api/{shopId}/arrivals/{id}")]
        public async Task<ArrivalRequestModel> GetOne(int id) => await _service.GetOne(id);

        [HttpPost("/api/{shopId}/arrivals")]
        public async Task<ArrivalRequestModel> Create([FromBody] ArrivalRequestModel model) => await _service.Create(model);

        [HttpPut("/api/{shopId}/arrivals/{id}")]
        public async Task<ArrivalRequestModel> Update(int id, [FromBody] ArrivalRequestModel model) => await _service.Edit(model);

        [HttpDelete("/api/{shopId}/arrivals/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.Remove(id);
            return Ok();
        }
    }
}
