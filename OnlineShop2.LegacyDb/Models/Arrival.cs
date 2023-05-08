using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShop2.LegacyDb.Models
{
    public class ArrivalLegacy
    {
        public int Id { get; set; }
        public DocumentStatusLegacy Status { get; set; } = DocumentStatusLegacy.Confirm;
        public string Num { get; set; }
        public DateTime DateArrival { get; set; }
        public int SupplierId { get; set; }
        public int ShopId { get; set; }
        public decimal SumArrival { get; set; }
        public decimal SumNds { get; set; }
        public decimal SumSell { get; set; }
        public decimal SumPayments { get; set; }
        public bool isSuccess { get; set; } = false;
        public List<ArrivalGoodLegacy> ArrivalGoods { get; set; }
    }

    public class ArrivalGoodLegacy
    {
        public int Id { get; set; }
        public int GoodId { get; set; }
        public decimal Count { get; set; }
        public decimal Price { get; set; }
        public string Nds { get; set; }
        public decimal PriceSell { get; set; }
        public decimal Sum { get; set; }
        public decimal SumNds { get; set; }
        public decimal SumSell { get; set; }
        public DateTime? ExpiresDate { get; set; }
    }
}
