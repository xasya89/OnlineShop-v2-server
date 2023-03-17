using AutoMapper;
using OnlineShop2.Database.Models;
using OnlineShop2.LegacyDb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShop2.LegacyDb.Infrastructure.MapperConfigurations
{
    public static class MapperInstance
    {
        private static readonly IMapper _instance;
        static MapperInstance()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<GoodGroupLegacy, GoodGroup>();
                cfg.CreateMap<SupplierLegacy, Supplier>();
                cfg.CreateMap<GoodLegacy, Good>();
                cfg.CreateMap<GoodPriceLegacy, GoodPrice>();
                cfg.CreateMap<BarCodeLegacy, Barcode>();
                cfg.CreateMap<GoodCountBalanceCurrentLegacy, GoodCurrentBalance>();
            });
            _instance = config.CreateMapper();
        }
        public static IMapper GetMapper() => _instance;
    }
}
