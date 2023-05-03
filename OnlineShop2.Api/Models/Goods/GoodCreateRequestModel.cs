using OnlineShop2.Dao;
using OnlineShop2.Database.Models;

namespace OnlineShop2.Api.Models.Goods
{
    public class GoodGroupCreateRequestModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ShopId { get; set; }
        public int? LegacyId { get; set; }
    }

    public class GoodCreateRequestModel
    {
        public int Id { get; set; }
        public int GoodGroupId { get; set; }
        public int? SupplierId { get; set; }
        public string Name { get; set; }
        public string? Article { get; set; }
        public Units Unit { get; set; }
        public decimal Price { get; set; }
        public SpecialTypes SpecialType { get; set; } = SpecialTypes.None;
        public double? VPackage { get; set; }
        public bool IsDeleted { get; set; } = false;

        public IEnumerable<GoodPriceCreateRequestModel> GoodPrices { get; set; }
        public IEnumerable<BarcodeCreateRequestModel> Barcodes { get; set; }
    }

    public class BarcodeCreateRequestModel
    {
        public int Id { get; set; }
        public bool IsDeleted { get; set; }
        public string Code { get; set; }
    }

    public class GoodPriceCreateRequestModel
    {
        public int Id { get; set; }
        public int ShopId { get; set; }
        public decimal Price { get; set; }
    }
}
