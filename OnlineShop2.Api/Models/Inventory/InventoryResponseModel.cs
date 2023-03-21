using OnlineShop2.Api.Models.Goods;
using OnlineShop2.Database.Models;

namespace OnlineShop2.Api.Models.Inventory
{
    public class InventoryResponseModel
    {
        public int Id { get; set; }
        public DateTime Start { get; set; }
        public string StartStr { get => Start.ToString("dd.MM.yy"); }
        public DateTime? Stop { get; set; }
        public string StopStr { get => Stop?.ToString("dd.MM.yy") ?? ""; }
        public int ShopId { get; set; }
        public decimal SumDb { get; set; }
        public decimal SumFact { get; set; }
        public decimal CashMoneyFact { get; set; }
        public decimal CashMoneyDb { get; set; }
        public DocumentStatus Status { get; set; }

        public List<InventoryGroupResponseModel> InventoryGroups { get; set; }
    }

    public class InventoryGroupResponseModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<InventoryGoodResponseModel> InventoryGoods { get; set; }
    }

    public class InventoryGoodResponseModel
    {
        public int Id { get; set; }
        public int InventoryGroupId { get; set; }
        public int GroupId { get => InventoryGroupId; }
        public int GoodId { get; set; }
        public GoodResponseModel? Good { get; set; }
        public string GoodName { get => Good?.Name ?? ""; }
        public decimal? CountFact { get; set; }
        public decimal Price { get => Good?.Price ?? 0; }
    }
}
