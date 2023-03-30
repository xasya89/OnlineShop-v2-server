using OnlineShop2.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OnlineShop2.LegacyDb.Models
{

    public class ShiftLegacy
    {
        public int Id { get; set; }
        public Guid Uuid { get; set; }
        public DateTime Start { get; set; } = DateTime.Now;
        public DateTime? Stop { get; set; }
        public decimal SumAll { get; set; } = 0;
        public decimal SumNoElectron { get; set; } = 0;
        public decimal SumElectron { get; set; } = 0;
        public decimal SumSell { get; set; } = 0;
        public decimal SumDiscount { get; set; } = 0;
        public decimal SummReturn
        {
            get => SumReturnCash + SumReturnElectron;
        }
        public decimal SumReturnCash { get; set; } = 0;
        public decimal SumReturnNoElectron { get => SumReturnCash; }
        public decimal SumReturnElectron { get; set; } = 0;
        public decimal SumIncome { get; set; } = 0;
        public decimal SumOutcome { get; set; } = 0;
        public int ShopId { get; set; }
        public List<CheckSellLegacy> CheckSells { get; set; } = new List<CheckSellLegacy>();
        public List<ShiftSaleLegacy> ShiftSales { get; set; } = new List<ShiftSaleLegacy>();
    }
    public class ShiftSaleLegacy
    {
        public int Id { get; set; }
        public int ShiftId { get; set; }
        public int GoodId { get; set; }
        public double Count { get; set; }
        public decimal Price { get; set; }
        public decimal Sum { get => (decimal)Count * Price; }
        public decimal CountReturn { get; set; }
        public decimal PriceReturn { get; set; }
        public decimal SumReturn { get => CountReturn * PriceReturn; }
    }

    public class CheckSellLegacy
    {
        public int Id { get; set; }
        public DateTime DateCreate { get; set; } = DateTime.Now;
        public TypeSell TypeSell { get; set; } = TypeSell.Sell;
        public int? BuyerId { get; set; }
        public string? BuyerName { get; set; }
        public string? BuyerPhone { get; set; }
        public bool IsElectron { get; set; }
        public decimal SumCash { get; set; }
        public decimal SumElectron { get; set; }
        public decimal Sum { get; set; }
        public decimal SumDiscont { get; set; } = 0;
        public decimal SumAll { get; set; }
        public List<CheckGoodLegacy> CheckGoods { get; set; } = new List<CheckGoodLegacy>();
        public int ShiftId { get; set; }
        public Shift Shift { get; set; }
    }

    public class CheckGoodLegacy
    {
        public int Id { get; set; }
        public decimal Count { get; set; }
        public decimal Price { get; set; }
        public int GoodId { get; set; }
        public Good Good { get; set; }
        public decimal Sum { get => Count * Price; }
        public int CheckSellId { get; set; }
    }
}
