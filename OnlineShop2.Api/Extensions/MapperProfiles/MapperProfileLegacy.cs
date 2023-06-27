using AutoMapper;
using OnlineShop2.Database.Models;
using OnlineShop2.LegacyDb.Models;

namespace OnlineShop2.Api.Extensions.MapperProfiles
{
    public class MapperProfileLegacy:Profile
    {
        public MapperProfileLegacy()
        {
            CreateMap<GoodGroupLegacy, GoodGroup>();
            CreateMap<SupplierLegacy, Supplier>();
            CreateMap<GoodLegacy, Good>();
            CreateMap<GoodPriceLegacy, GoodPrice>();
            CreateMap<BarCodeLegacy, Barcode>();
            CreateMap<GoodCountBalanceCurrentLegacy, GoodCurrentBalance>();
            CreateMap<GoodGroup, GoodGroupLegacy>();
            CreateMap<Supplier, SupplierLegacy>();
            CreateMap<Good, GoodLegacy>();
            CreateMap<GoodPrice, GoodPriceLegacy>();
            CreateMap<Barcode, BarCodeLegacy>();
            CreateMap<GoodCurrentBalance, GoodCountBalanceCurrentLegacy>();
            CreateMap<Arrival, ArrivalLegacy>();
            CreateMap<ArrivalGood, ArrivalGoodLegacy>();

            CreateMap<WriteofLegacy, Writeof>();
            CreateMap<WriteofGoodLegacy, WriteofGood>();
            CreateMap<Writeof, WriteofLegacy>();
            CreateMap<WriteofGood, WriteofGoodLegacy>();

            CreateMap<MoneyReportLegacy, MoneyReport>();
        }
    }
}
