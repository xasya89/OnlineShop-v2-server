using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineShop2.Api.Services.Legacy;

namespace OnlineShop2.Api.Controllers.Inventory
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryController : ControllerBase
    {
        private readonly SynchLegacyService _service;
        private readonly InventoryLegacyService _inventoryService;
        public InventoryController(SynchLegacyService service, InventoryLegacyService inventoryService) 
        {
            _service = service;
            _inventoryService = inventoryService;
        }

        [HttpGet("/api/{shopId:int}/inventory/start")]
        public async Task<ActionResult> Start(int shopId=1, int shopLegacy=7)
        {
            await _service.SynchGoods(shopId, shopLegacy);
            await _inventoryService.Start(shopId, shopLegacy);
            return Ok();
        }
    }
}
