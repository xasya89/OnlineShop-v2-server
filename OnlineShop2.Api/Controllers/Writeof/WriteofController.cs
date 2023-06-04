using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineShop2.Api.Models.Writeof;
using OnlineShop2.Api.Services;

namespace OnlineShop2.Api.Controllers.Writeof
{
    [Route("api/[controller]")]
    [ApiController]
    public class WriteofController : ControllerBase
    {
        private readonly WriteofService _writeofService;

        public WriteofController(WriteofService writeofService)
        {
            _writeofService = writeofService;
        }

        [HttpGet("/api/{shopId}/writeofs")]
        public async Task<IActionResult> GetAll(int shopId, [FromQuery] int page, [FromQuery] int count)
            => Ok(await _writeofService.GetAll(shopId, page, count));

        [HttpGet("/api/{shopId}/writeofs/{id}")]
        public async Task<WriteofModel> GetOne(int shopId, int id) =>
            await _writeofService.GetOne(id);

        [HttpPost("/api/{shopId}/writeofs")]
        public async Task<WriteofModel> Add(int shopId, [FromBody] WriteofModel model) =>
            await _writeofService.Add(model);

        [HttpPut("/api/{shopId}/writeofs")]
        public async Task<WriteofModel> Update([FromBody] WriteofModel model) =>
            await _writeofService.Update(model);

        [HttpDelete("/api/{shopId}/writeofs/{id}")]
        public async Task Delete(int id) =>
            await _writeofService.Delete(id);
    }
}
