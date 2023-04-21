using Microsoft.EntityFrameworkCore;
using OnlineShop2.Api.Models.Goods;
using OnlineShop2.Api.Services.Legacy;
using OnlineShop2.Database;
using OnlineShop2.Database.Models;
using OnlineShop2.LegacyDb;

namespace OnlineShop2.Api.Services
{
    public class SupplierService
    {
        private readonly IConfiguration _configuration;
        private readonly OnlineShopContext _context;

        public SupplierService(OnlineShopContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<IEnumerable<SupplierResponseModel>> GetList(int shopId)
        {
            var suppliers = _context.Suppliers;
            if (_configuration.GetValue<bool>("OwnerGoodForShops"))
                suppliers.Where(s => s.ShopId==shopId);
            return await suppliers.AsNoTracking().OrderBy(s=>s.Name).Select(s => new SupplierResponseModel
            {
                Id = s.Id,
                ShopId = s.ShopId,
                Name = s.Name
            }).ToListAsync();
        }
    }
}
