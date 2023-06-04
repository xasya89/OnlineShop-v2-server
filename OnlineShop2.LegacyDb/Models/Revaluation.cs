using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShop2.LegacyDb.Models
{
    public class RevaluationLegacy
    {
        public int Id { get; set; }
        public DateTime Create { get; set; }
        public decimal SumOld { get; set; }
        public decimal SumNew { get; set; }
        public List<RevaluationGoodLegacy> RevaluationGoods { get; set; } = new();
    }

    public class RevaluationGoodLegacy
    {
        public int Id { get; set; }
        public int RevaluationId { get; set; }
        public int GoodId { get; set; }
        public decimal Count { get; set; }
        public decimal PriceOld { get; set; }
        public decimal PriceNew { get; set; }
    }
}
