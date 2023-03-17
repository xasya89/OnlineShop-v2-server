using OnlineShop2.Database;
using OnlineShop2.Database.Models;

namespace OnlineShop2.Api.Services
{
    public class ShopService
    {
        private readonly OnlineShopContext _context;
        public ShopService(OnlineShopContext context)
        {
            _context = context;
        }

        public IEnumerable<Shop> GetShops() => _context.Shops.OrderBy(s => s.Alias);
    }
}
