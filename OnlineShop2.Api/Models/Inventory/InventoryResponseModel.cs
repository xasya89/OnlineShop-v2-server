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
        public int Total { get; set; }
        public List<InventorySummaryGoodResponseModel> InventorySummaryGoods { get; set; }
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
        public Good Good { private get; set; }
        public string GoodName { get => Good?.Name ?? ""; }
        public decimal? CountFact { get; set; }
        public decimal Price { get => Good?.Price ?? 0; }
    }

    public class InventorySummaryGoodResponseModel
    {
        public int Id { get; set; }
        public int GoodId { get; set; }
        public string GoodName { get => Good?.Name ?? ""; }
        public Good Good { get; set; }
        public decimal Price { get; set; } = 0;
        public decimal CountOld { get; set; } = 0;
        public decimal CountCurrent { get; set; } = 0;
        public decimal CountDiff { get => CountCurrent - CountOld; }
        public decimal PriceDiif { get => (CountCurrent - CountOld) * Price; }
    }
}
