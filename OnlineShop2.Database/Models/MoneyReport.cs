using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShop2.Database.Models
{
    public class MoneyReport
    {
        public int Id { get; set; }
        public Shop Shop { get; set; }
        public int ShopId { get; set; }
        public DateTime Create { get; set; } = DateTime.Now;
        public decimal? InventoryGoodsSum { get; set; }
        public decimal? InventoryCashMoney { get; set; }
        public decimal? ArrivalsSum { get; set; }
        public decimal? CashIncome { get; set; }
        public decimal? CashOutcome { get; set; }
        public decimal? CashMoney { get; set; }
        public decimal? CashElectron { get; set; }
        public decimal? Writeof { get; set; }
        public decimal? RevaluationOld { get; set; }
        public decimal? RevaluationNew { get; set; }
        public decimal? StopGoodSum { get; set; }
    }
}
