using AutoMapper;
using OnlineShop2.Api.Models.Goods;
using OnlineShop2.Api.Models.Inventory;
using OnlineShop2.Api.Models.Shop;
using OnlineShop2.Database.Models;

namespace OnlineShop2.Api.Extensions
{
    public class MapperConfigurationExtension
    {
        private static readonly IMapper _instance;
        static MapperConfigurationExtension()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Shop, ShopResponseModel>();
                cfg.CreateMap<Good, GoodResponseModel>();
                cfg.CreateMap<Inventory, InventoryResponseModel>();
                cfg.CreateMap<InventoryGroup, InventoryGroupResponseModel>();
            });
            _instance = config.CreateMapper();
        }
        public static IMapper GetMapper() => _instance;
    }
}
