using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineShop2.Api.Models.Goods;
using OnlineShop2.Api.Services;

namespace OnlineShop2.Api.Controllers.Goods
{
    [Route("api/[controller]")]
    [ApiController]
    public class GoodGroupController : ControllerBase
    {
        private readonly GoodGroupService _service;

        public GoodGroupController(GoodGroupService service)
        {
            _service = service;
        }

        [HttpGet("/api/{shopId}/goodgroups")]
        public async Task<IEnumerable<GoodGroupCreateRequestModel>> GetAll(int shopId) => await _service.GetAll(shopId);

        [HttpPost("/api/{shopId}/goodgroups")]
        public async Task<GoodGroupCreateRequestModel> Create(int shopId, [FromBody]GoodGroupCreateRequestModel model) =>
            await _service.Create(shopId, model);

        [HttpPut("/api/{shopId}/goodgroups/{id}")]
        public async Task<GoodGroupCreateRequestModel> Update(int shopId, [FromBody] GoodGroupCreateRequestModel model) =>
            await _service.Update(model);

        [HttpDelete("/api/{shopId}/goodgroups/{id}")]
        public async Task Delete(int id) =>
            await _service.Delete(id);
    }
}
