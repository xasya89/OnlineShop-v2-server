using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineShop2.Api.Models.Inventory;
using OnlineShop2.Api.Services.Legacy;

namespace OnlineShop2.Api.Controllers.Inventory
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryGroupsController : ControllerBase
    {
        private readonly SynchLegacyService _service;
        private readonly InventoryLegacyService _inventoryService;
        public InventoryGroupsController(SynchLegacyService service, InventoryLegacyService inventoryService)
        {
            _service = service;
            _inventoryService = inventoryService;
        }

        [HttpPost("/api/{shopId}/inventory/{id}/groups")]
        public async Task<IActionResult> AddGroup(int id, [FromBody] InventoryAddGroupRequestModel model) =>
            Ok(await _inventoryService.AddGroup(id, model));

        [HttpPut("/api/{shopId}/inventory/{id}/groups/{groupId}")]
        public async Task<InventoryGroupResponseModel> Edit(int groupId, [FromBody] InventoryAddGroupRequestModel model) =>
            await _inventoryService.EditGroup(groupId, model);

        [HttpDelete("/api/{shopId}/inventory/{id}/groups/{groupId}")]
        public async Task<IActionResult> Remove(int groupId)
        {
            await _inventoryService.RemoveGroup(groupId);
            return Ok();
        }
    }
}
