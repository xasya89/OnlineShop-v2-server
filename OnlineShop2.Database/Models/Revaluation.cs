using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShop2.Database.Models
{
    public class Revaluation
    {
        public int Id { get; set; }
        public Shop Shop { get; set; }
        public int ShopId { get; set; }
        public DocumentStatus Status { get; set; } = DocumentStatus.Successed;
        public DateTime Create { get; set; }
        public decimal SumOld { get; set; }
        public decimal SumNew { get; set; }
        public string Note { get; set; }
        public int? ParentId { get; set; }
        public DocumentType? DocumentType { get; set; }
        public int? LegacyId { get; set; }

        public IEnumerable<RevaluationGood> RevaluationGoods { get; set; }
    }

    public class RevaluationGood
    {
        public int Id { get; set; }
        public Revaluation Revaluation { get; set; }
        public int RevaluationId { get; set; }
        public Good Good { get; set; }
        public int GoodId { get; set; }
        public decimal Count { get; set; }
        public decimal PriceOld { get; set; }
        public decimal PriceNew { get; set; }
    }
}
