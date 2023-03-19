﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineShop2.Api.Models.Goods;
using OnlineShop2.Api.Services;
using OnlineShop2.Database;

namespace OnlineShop2.Api.Controllers.Goods
{
    [Route("api/[controller]")]
    [ApiController]
    public class GoodController : ControllerBase
    {
        private readonly GoodService _service;
        public GoodController(GoodService service) => _service = service;

        [HttpGet("/api/{shopId}/goods/scan/{barcode}")]
        public async Task<GoodResponseModel> Scan(int shopId, string barcode)=>await _service.GetGoodByBarcode(shopId, barcode);
    }
}
