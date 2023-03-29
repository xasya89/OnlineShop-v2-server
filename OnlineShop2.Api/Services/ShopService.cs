using OnlineShop2.Api.Extensions;
using OnlineShop2.Api.Models.Shop;
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

        public IEnumerable<ShopResponseModel> GetShops() => 
            MapperConfigurationExtension.GetMapper().Map<IEnumerable<Shop>, IEnumerable<ShopResponseModel>>(_context.Shops.OrderBy(s => s.Alias));

        public ShopResponseModel GetShop(int id) =>
            MapperConfigurationExtension.GetMapper().Map<Shop, ShopResponseModel>(_context.Shops.Find(id));
    }
}
