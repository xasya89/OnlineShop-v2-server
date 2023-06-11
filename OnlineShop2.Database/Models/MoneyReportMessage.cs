using OnlineShop2.Dao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShop2.Database.Models
{
    public class MoneyReportMessage
    {
        public int Id { get; set; }
        public DateTime DateRecive { get; set; } = DateTime.Now;
        public MoneyReportMessageTypeDoc TypeDoc { get; set; }
        public DateTime Date { get; set; }
        public Shop Shop { get; set; }
        public int ShopId { get; set; }
        public int DocId { get; set; }
        public decimal? Sum { get; set; } = null;
        public string? Error { get; set; } = null;
    }
}
