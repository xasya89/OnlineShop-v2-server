using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineShop2.Api.Models.Goods;
using OnlineShop2.Api.Services;
using OnlineShop2.Database;

namespace OnlineShop2.Api.Controllers.Goods
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class GoodController : ControllerBase
    {
        private readonly GoodService _service;
        public GoodController(GoodService service) => _service = service;

        [HttpGet("/api/{shopId}/goods/{id}")]
        public async Task<GoodResponseModel> Get(int shopId, int id) => await _service.Get(shopId, id);

        [HttpGet("/api/{shopId}/goods/scan/{barcode}")]
        public async Task<GoodResponseModel> Scan(int shopId, string barcode)=>await _service.GetGoodByBarcode(shopId, barcode);

        [HttpPost("/api/{shopId}/goods")]
        public async Task<GoodResponseModel> Create(int shopId, [FromBody]GoodCreateRequestModel model)=>await _service.Create(shopId, model);

        [HttpPut("/api/{shopId}/goods")]
        public async Task<GoodResponseModel> Update(int shopId, [FromBody] GoodCreateRequestModel model)=> await _service.Update(shopId, model);
    }
}
