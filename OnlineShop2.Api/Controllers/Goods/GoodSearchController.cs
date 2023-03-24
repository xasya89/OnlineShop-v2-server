using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineShop2.Api.Models.Goods;
using OnlineShop2.Api.Services;

namespace OnlineShop2.Api.Controllers.Goods
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class GoodSearchController : ControllerBase
    {
        private readonly GoodService _service;
        public GoodSearchController(GoodService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IEnumerable<GoodResponseModel>> Search([FromQuery] string search) =>
            _service.Search(search);
    }
}
