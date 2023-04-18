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
        private readonly SynchLegacyService _synchLegacy;

        public SupplierService(OnlineShopContext context, IConfiguration configuration, SynchLegacyService synchLegacy)
        {
            _context = context;
            _configuration = configuration;
            _synchLegacy=synchLegacy;
        }

        public async Task<IEnumerable<SupplierResponseModel>> GetList(int shopId)
        {
            await _synchLegacy.SynchSuppliers(shopId);
            return await _context.Suppliers.Where(s => s.ShopId == shopId).AsNoTracking().OrderBy(s=>s.Name).Select(s => new SupplierResponseModel
            {
                Id = s.Id,
                ShopId = s.ShopId,
                Name = s.Name
            }).ToListAsync();
        }
    }
}
