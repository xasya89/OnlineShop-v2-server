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

        [HttpGet("/api/{shopId}/suppliers/{id}")]
        public async Task<SupplierResponseModel> GetOne(int id) => await _supplierService.Get(id);

        [HttpPost("/api/{shopId}/suppliers")]
        public async Task<SupplierResponseModel> Post(int shopId, SupplierResponseModel model) => await _supplierService.Add(shopId, model);

        [HttpPut("/api/{shopId}/suppliers")]
        public async Task<SupplierResponseModel> Put(int shopId, SupplierResponseModel model) => await _supplierService.Update(model);

        [HttpDelete("/api/{shopId}/suppliers/{id}")]
        public async Task Delete(int shopId, int id) => await _supplierService.Delete(id);
    }
}
