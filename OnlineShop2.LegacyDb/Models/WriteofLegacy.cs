using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShop2.LegacyDb.Models
{
    public class WriteofLegacy
    {
        public int Id { get; set; }
        public Guid? Uuid { get; set; } = null;
        public DocumentStatusLegacy Status { get; set; } = DocumentStatusLegacy.Confirm;
        public DateTime DateWriteof { get; set; }
        public string Note { get; set; }
        public decimal SumAll { get; set; }
        public bool IsSuccess { get; set; }
        public List<WriteofGoodLegacy> WriteofGoods { get; set; } = new List<WriteofGoodLegacy>();
    }
    public class WriteofGoodLegacy
    {
        public int Id { get; set; }
        public int WriteofId { get; set; }
        public int GoodId { get; set; }
        public decimal Count { get; set; }
        public decimal Price { get; set; }
    }
}
