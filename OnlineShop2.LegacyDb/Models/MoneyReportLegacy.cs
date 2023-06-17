using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShop2.LegacyDb.Models
{
    public class MoneyReportLegacy
    {
        public DateTime Create { get; set; }
        public decimal StartGoodSum { get; set; } = 0;
        public decimal StartCashMoney { get; set; } = 0;
        public decimal InventoryGoodsSum { get; set; }
        public decimal InventoryCashMoney { get; set; }
        public decimal ArrivalsSum { get; set; }
        public decimal CashIncome { get; set; }
        public decimal CashOutcome { get; set; }
        public decimal CashMoney { get; set; }
        public decimal CashElectron { get; set; }
        public decimal Writeof { get; set; }
        public decimal RevaluationOld { get; set; }
        public decimal RevaluationNew { get; set; }
        public decimal MoneyItog { get; set; }
        public decimal StopGoodSum { get; set; }
    }
}
