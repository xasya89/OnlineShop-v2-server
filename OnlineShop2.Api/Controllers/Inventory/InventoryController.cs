﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineShop2.Api.Models.Inventory;
using OnlineShop2.Api.Services;
using OnlineShop2.Api.Services.Legacy;
using OnlineShop2.Database.Models;

namespace OnlineShop2.Api.Controllers.Inventory
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class InventoryController : ControllerBase
    {
        private readonly InventoryLegacyService _inventoryService;
        private readonly ShopService _shopService;
        public InventoryController( InventoryLegacyService inventoryService, ShopService shopService) 
        {
            _inventoryService = inventoryService;
            _shopService = shopService;
        }

        [HttpGet("/api/{shopId}/inventory")]
        public async Task<IEnumerable<InventoryResponseModel>> GetList(int shopId)=>
            await _inventoryService.GetList(shopId);

        [HttpPost("/api/{shopId:int}/inventory")]
        public async Task<IActionResult> Start(int shopId, [FromBody] InventoryStartRequestModel model)
        {
            int shopLegacy = _shopService.GetShop(shopId).LegacyDbNum ?? 0;
            var result = await _inventoryService.Start(shopId, shopLegacy, model);
            return Ok(result);
        }

        [HttpPost("/api/{shopId:int}/inventory/{id}/complite")]
        public async Task<IActionResult> Complite(int id, [FromBody] IEnumerable<InventoryAddGoodRequestModel> model)
        {
            await _inventoryService.Complite(id, model);
            return Ok();
        }

        [HttpPost("/api/{shopId:int}/inventory/{id}/setmoney")]
        public async Task<IActionResult> CompliteSetMoney(int shopId, int id, [FromBody] InventoryStartRequestModel model)
        {
            await _inventoryService.CompliteSetMoneyFact(id, model);
            return Ok();
        }

        [HttpGet("/api/{shopId}/inventory/{id}")]
        public async Task<IActionResult> GetInventory(int shopId, int id) =>
            Ok(await _inventoryService.GetInventory(shopId, id));

        [HttpGet("/api/{shopId}/inventory-view/{id}")]
        public async Task<InventoryResponseModel> GetInventoryComplite(int shopId, int id, [FromQuery]string? search = "", [FromQuery]int page = 1, [FromQuery] int pageSize = 10000, [FromQuery] bool isDiff=false) =>
            await _inventoryService.GetInventoryComplite(shopId, id, search, page, pageSize, isDiff );

        [HttpPost("/api/{shopId}/inventory/{id}/addgroup")]
        public async Task<IActionResult> AddGroup(int id, [FromBody] InventoryAddGroupRequestModel model) =>
            Ok(await _inventoryService.AddGroup(id, model));

        [HttpPost("/api/{shopId}/inventory/{id}/goods")]
        public async Task<IEnumerable<InventoryGoodResponseModel>> AddEditGoods(int id, [FromBody] IEnumerable<InventoryAddGoodRequestModel> model) =>
            await _inventoryService.AddEditGood(id, model);


        [HttpDelete("/api/{shopId}/inventory/{id}")]
        public async Task<IActionResult> Remove(int shopId, int id)
        {
            await _inventoryService.RemoveInventory(shopId, id);
            return Ok();
        }
    }
}
