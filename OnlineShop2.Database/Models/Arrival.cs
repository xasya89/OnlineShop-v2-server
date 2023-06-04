using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShop2.Database.Models
{
    public class Arrival
    {
        public int Id { get; set; }
        public DocumentStatus Status { get; set; } = DocumentStatus.New;
        public string Num { get; set; }
        public DateTime DateArrival { get; set; } = DateTime.Now;
        public int SupplierId { get; set; }
        public Supplier Supplier { get; set; }
        public int ShopId { get; set; }
        public Shop Shop { get; set; }
        public decimal PurchaseAmount { get; set; }
        public decimal SumNds { get; set; }
        public decimal SaleAmount { get; set; }
        public int? LegacyId { get; set; }

        public List<ArrivalGood> ArrivalGoods { get; set; } = new();
    }

    public class ArrivalGood
    {
        public int Id { get; set; }
        public int ArrivalId { get; set; }
        public Arrival Arrival { get; set; }
        public int SequenceNum { get; set; }
        public int GoodId { get; set; }
        public Good Good { get; set; }
        public decimal Count { get; set; }
        public decimal PricePurchase { get; set; }
        public NDSType Nds { get; set; } = NDSType.None;
        public decimal PriceSell { get; set; }
        public DateTime? ExpiresDate { get; set; }
    }

    public enum NDSType
    {
        None,
        Percent_20,
        Percent_10,
        Percent_0
    }
}
