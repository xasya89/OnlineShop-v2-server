using OnlineShop2.Dao;
using OnlineShop2.Database.Models;

namespace OnlineShop2.Api.Models.Goods
{
    public class GoodGroupResponseModel
    {
        public int Id { get; set; }
        public int ShopId { get; set; }
        public string Name { get; set; }
    }

    public class GoodResponseModel
    {
        public int Id { get; set; }
        public int GoodGroupId { get; set; }
        public GoodGroupResponseModel GoodGroup { get; set; }
        public int? SupplierId { get; set; }
        public SupplierResponseModel? Supplier { get; set; }
        public string Name { get; set; }
        public string? Article { get; set; }
        public Units Unit { get; set; }
        public decimal Price { get; set; }
        public SpecialTypes SpecialType { get; set; }
        public double? VPackage { get; set; }
        public bool IsDeleted { get; set; }
        public Guid Uuid { get; set; } 

        public IEnumerable<GoodPriceResponseModel> GoodPrices { get; set; }
        public IEnumerable<BarCodeResponseModel> Barcodes { get; set; }
    }

    public class GoodPriceResponseModel
    {
        public int Id { get; set; }
        public int ShopId { get; set; }
        public decimal Price { get; set; }
    }

    public class BarCodeResponseModel
    {
        public int Id { get; set; }
        public string Code { get; set; }
    }
}
