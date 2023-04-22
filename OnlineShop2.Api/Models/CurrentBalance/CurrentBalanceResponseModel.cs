using OnlineShop2.Api.Models.Goods;
using OnlineShop2.Database.Models;

namespace OnlineShop2.Api.Models.CurrentBalance
{
    public class CurrentBalanceResponseModel
    {
        public int Id { get; set; }
        public int ShopId { get; set; }
        public int GoodId { get; set; }
        public string Name { get; set; }
        public Units Unit { get; set; }
        public decimal? Price { get; set; }
        public int GoodGroupId { get; set; }
        public int? SupplierId { get; set; }
        public decimal CurrentCount { get; set; }
    }
}
