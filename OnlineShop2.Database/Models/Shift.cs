using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OnlineShop2.Database.Models
{
    public class Shift
    {
        public int Id { get; set; }
        public int ShopId { get; set; }
        public Shop Shop { get; set; }
        public Guid Uuid { get; set; }
        public DateTime Start { get; set; } = DateTime.Now;
        public DateTime? Stop { get; set; }
        public decimal SumAll { get; set; } = 0;
        public decimal SumNoElectron { get; set; } = 0;
        public decimal SumElectron { get; set; } = 0;
        public decimal SumSell { get; set; } = 0;
        public decimal SumDiscount { get; set; } = 0;
        public decimal SumReturnNoElectron { get; set; } = 0;
        public decimal SumReturnElectron { get; set; } = 0;
        public decimal SumIncome { get; set; } = 0;
        public decimal SumOutcome { get; set; } = 0;
        public string? CashierName { get; set; }
        public int? LegacyId { get; set; }
        public List<CheckSell> CheckSells { get; set; } = new();
        public List<ShiftSummary> ShiftSummaries { get; set; } = new();
    }

    public class ShiftSummary
    {
        public int Id { get; set; }
        public int ShiftId { get; set; }
        public Shift Shift { get; set; }
        public int GoodId { get; set; }
        public Good Good { get; set; }
        public double Count { get; set; }
        public decimal Sum { get; set; }
        public decimal CountReturn { get; set; }
        public decimal SumReturn { get; set; }
        public int? LegacyId { get; set; }
    }

    public class CheckSell
    {
        public int Id { get; set; }
        public int ShiftId { get; set; }
        public Shift Shift { get; set; }
        public DateTime DateCreate { get; set; } = DateTime.Now;
        public TypeSell TypeSell { get; set; } = TypeSell.Sell;
        public int? BuyerId { get; set; }
        public string? BuyerName { get; set; }
        public string? BuyerPhone { get; set; }
        public decimal SumBuy { get; set; } // SumAll
        public decimal SumDiscont { get; set; } = 0;
        public decimal SumNoElectron { get; set; }
        public decimal SumElectron { get; set; }
        public int? LegacyId { get; set; }

        public List<CheckGood> CheckGoods { get; set; } = new();

        public ICollection<InventoryAppendCheck> InventoryAppendChecks { get; set; }
    }

    public class CheckGood
    {
        public int Id { get; set; }
        public int CheckSellId { get; set; }
        public CheckSell CheckSell { get; set; }
        public int GoodId { get; set; }
        public Good Good { get; set; }
        public decimal Count { get; set; }
        public decimal Price { get; set; }
    }

    public enum TypeSell
    {
        Sell = 0,
        Return = 1
    }
}
