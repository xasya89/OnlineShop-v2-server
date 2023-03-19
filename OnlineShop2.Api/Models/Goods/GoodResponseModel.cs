using OnlineShop2.Database.Models;

namespace OnlineShop2.Api.Models.Goods
{
    public class GoodResponseModel
    {
        public int Id { get; set; }
        public int GoodGroupId { get; set; }
        public GoodGroup GoodGroup { get; set; }
        public int? SupplierId { get; set; }
        public Supplier? Supplier { get; set; }
        public string Name { get; set; }
        public string? Article { get; set; }
        public Units Unit { get; set; }
        public decimal Price { get; set; }
        public SpecialTypes SpecialType { get; set; }
        public double? VPackage { get; set; }
        public bool IsDeleted { get; set; }
        public Guid Uuid { get; set; } 

    }
}
