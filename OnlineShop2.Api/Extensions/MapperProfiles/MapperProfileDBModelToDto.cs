using AutoMapper;
using OnlineShop2.Api.Models.Arrival;
using OnlineShop2.Api.Models.CurrentBalance;
using OnlineShop2.Api.Models.Goods;
using OnlineShop2.Api.Models.Inventory;
using OnlineShop2.Api.Models.Shop;
using OnlineShop2.Database.Models;

namespace OnlineShop2.Api.Extensions.MapperProfiles
{
    public class MapperProfileDBModelToDto: Profile
    {
        public MapperProfileDBModelToDto()
        {
            CreateMap<Shop, ShopResponseModel>();
            CreateMap<GoodGroup, GoodGroupResponseModel>();
            CreateMap<Supplier, SupplierResponseModel>();
            CreateMap<SupplierResponseModel, Supplier>();

            CreateMap<GoodGroup, GoodGroupCreateRequestModel>();
            CreateMap<Good, GoodResponseModel>();
            CreateMap<GoodPrice, GoodPriceResponseModel>();
            CreateMap<Barcode, BarCodeResponseModel>();

            CreateMap<GoodGroupCreateRequestModel, GoodGroup>();
            CreateMap<GoodCreateRequestModel, Good>();
            CreateMap<GoodPriceCreateRequestModel, GoodPrice>();
            CreateMap<BarcodeCreateRequestModel, Barcode>();

            CreateMap<Inventory, InventoryResponseModel>();
            CreateMap<InventoryGroup, InventoryGroupResponseModel>();
            CreateMap<InventoryGood, InventoryGoodResponseModel>();
            CreateMap<InventoryAddGoodRequestModel, InventoryGood>();
            CreateMap<InventorySummaryGood, InventorySummaryGoodResponseModel>();

            CreateMap<GoodCurrentBalance, CurrentBalanceResponseModel>();

            CreateMap<Arrival, ArrivalModel>();
            CreateMap<ArrivalGood, ArrivalGoodModel>();
            CreateMap<ArrivalModel, Arrival>();
            CreateMap<ArrivalGoodModel, ArrivalGood>();
            CreateMap<Arrival, ArrivalResponseModel>();
            CreateMap<ArrivalGood, ArrivalGoodResponseModel>();
        }
    }
}
