using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShop2.Database.Models
{
    public class Writeof
    {
        public int Id { get; set; }
        public DocumentStatus Status { get; set; } = DocumentStatus.Successed;
        public DateTime DateWriteof { get; set; }
        public int ShopId { get; set; }
        public Shop Shop { get; set; }
        public string Note { get; set; }
        public decimal SumAll { get; set; }
        public int? LegacyId { get; set; }

        public List<WriteofGood> WriteofGoods { get; set; }
    }

    public class WriteofGood
    {
        public int Id { get; set; }
        public int WriteofId { get; set; }
        public Writeof Writeof { get; set; }
        public int GoodId { get; set; }
        public Good Good { get; set; }
        public decimal Price { get; set; }
        public decimal Count { get; set; }
    }
}
