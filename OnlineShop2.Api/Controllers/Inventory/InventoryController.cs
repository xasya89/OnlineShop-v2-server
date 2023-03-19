using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineShop2.Api.Models.Inventory;
using OnlineShop2.Api.Services.Legacy;
using OnlineShop2.Database.Models;

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

        [HttpPost("/api/{shopId:int}/inventory/legacystart/{shopLegacy}")]
        public async Task<IActionResult> Start(int shopId=1, int shopLegacy=7)
        {
            await _service.SynchGoods(shopId, shopLegacy);
            var result = await _inventoryService.Start(shopId, shopLegacy);
            return Ok(result);
        }

        [HttpGet("/api/{shopId}/inventory/{id}")]
        public async Task<IActionResult> GetInventory(int shopId, int id) =>
            Ok(await _inventoryService.GetInventory(shopId, id));

        [HttpPost("/api/{shopId}/inventory/{id}/addgroup")]
        public async Task<ActionResult> AddGroup(int id, [FromBody] InventoryAddGroupRequestModel model) =>
            Ok(await _inventoryService.AddGroup(id, model));

    }
}
