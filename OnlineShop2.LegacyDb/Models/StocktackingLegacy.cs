using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShop2.LegacyDb.Models
{
    public class StocktackingSummaryLegacy
    {
        public int Id { get; set; }
        public DateTime Create { get; set; }
        public DateTime Start { get; set; }
        public decimal SumDb { get; set; }
        public decimal SumFact { get; set; }
        public decimal CashMoneyDb { get; set; }
        public decimal CashMoneyFact { get; set; }
    }
}
