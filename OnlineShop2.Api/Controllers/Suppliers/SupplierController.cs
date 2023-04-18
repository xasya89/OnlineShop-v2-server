using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineShop2.Api.Models.Goods;
using OnlineShop2.Api.Services;

namespace OnlineShop2.Api.Controllers.Suppliers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupplierController : ControllerBase
    {
        private readonly SupplierService _supplierService;

        public SupplierController(SupplierService supplierService)
        {
            _supplierService = supplierService;
        }

        [HttpGet("/api/{shopId}/suppliers")]
        public async Task<IEnumerable<SupplierResponseModel>> Get(int shopId) => await _supplierService.GetList(shopId);
    }
}
