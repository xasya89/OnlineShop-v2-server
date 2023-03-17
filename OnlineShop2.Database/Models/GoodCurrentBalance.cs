using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShop2.Database.Models
{
    public class GoodCurrentBalance
    {
        public int Id { get; set; }
        public int ShopId { get; set; }
        public Shop Shop { get; set; }
        public int GoodId { get; set; }
        public Good Good { get; set; }
        public decimal CurrentCount { get; set; } = 0;
    }
}
