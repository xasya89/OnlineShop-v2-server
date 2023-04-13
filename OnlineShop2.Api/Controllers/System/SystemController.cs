using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace OnlineShop2.Api.Controllers.System
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SystemConfigurationController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public SystemConfigurationController(IConfiguration configuration) => _configuration = configuration;

        [HttpGet]
        public IActionResult Get() => Ok(new
        {
            InventoryShema = _configuration.GetValue<string>("InventoryShema"),
            OwnerGoodForShops = _configuration.GetValue<bool>("OwnerGoodForShops")
        });
    }
}
