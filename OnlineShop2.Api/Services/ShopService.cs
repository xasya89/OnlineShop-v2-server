using AutoMapper;
using OnlineShop2.Api.Extensions;
using OnlineShop2.Api.Models.Shop;
using OnlineShop2.Database;
using OnlineShop2.Database.Models;

namespace OnlineShop2.Api.Services
{
    public class ShopService
    {
        private readonly OnlineShopContext _context;
        private readonly IMapper _mapper;
        public ShopService(OnlineShopContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public IEnumerable<ShopResponseModel> GetShops() => 
            _mapper.Map<IEnumerable<Shop>, IEnumerable<ShopResponseModel>>(_context.Shops.OrderBy(s => s.Alias));

        public ShopResponseModel GetShop(int id) =>
            _mapper.Map<Shop, ShopResponseModel>(_context.Shops.Find(id));
    }
}
