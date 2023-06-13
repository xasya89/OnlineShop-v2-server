
using OnlineShop2.Dao;
using OnlineShop2.Dao.Extensions;
using OnlineShop2.Database.Models;

namespace OnlineShop2.Api.Models.ReportsModels
{
    public class ShiftWithChecksResponseModel: ShiftResponseModel
    {
        public decimal SumPercent { get => Math.Round((SumAll) * 0.05m); }
        public IEnumerable<CheckSellResponseModel> CheckSells { get; set; }
    }

    public class CheckSellResponseModel
    {
        public int Id { get; set; }
        public DateTime DateCreate { get; set; }
        public string DateCreateStr { get => DateCreate.ToString("dd.MM HH:mm"); }
        public TypeSell TypeSell { get; set; }
        public string TypeSellStr { get => TypeSell.GetDisplayName(); }
        public int? BuyerId { get; set; }
        public string? BuyerName { get; set; }
        public string? BuyerPhone { get; set; }
        public decimal SumBuy { get; set; }
        public decimal SumDiscont { get; set; }
        public decimal SumNoElectron { get; set; }
        public decimal SumElectron { get; set; }
        public IEnumerable<CheckGoodResponseModel> CheckGoods { get; set; }
    }

    public class CheckGoodResponseModel
    {
        public int Id { get; set; }
        public int GoodId { get; set; }
        public string GoodName { get; set; }
        public Units Unit { get; set; }
        public string UnitStr { get=>Unit.GetDisplayName(); }
        public decimal Count { get; set; }
        public decimal Price { get; set; }
    }
}
