using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineShop2.Api.Services;
using OnlineShop2.Database.Models;

namespace OnlineShop2.Api.Controllers.Shops
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ShopsController : ControllerBase
    {
        private readonly ShopService _service;
        public ShopsController(ShopService service) => _service = service;

        [HttpGet]
        public IEnumerable<Shop> Get() => _service.GetShops();
    }
}
