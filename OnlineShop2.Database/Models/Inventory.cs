using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShop2.Database.Models
{
    public class Inventory
    {
        public int Id { get; set; }
        public DateTime Start { get; set; } = DateTime.Now;
        public DateTime? Stop { get; set; }
        public int ShopId { get; set; }
        public Shop Shop { get; set; }
        public decimal SumDb { get; set; } = 0;
        public decimal SumFact { get; set; } = 0;
        public decimal CashMoneyFact { get; set; } = 0;
        public decimal CashMoneyDb { get; set; } = 0;
        public DocumentStatus Status { get; set; } = DocumentStatus.New;

        public List<InventoryGroup> InventoryGroups { get; set; }
    }

    public class InventoryGroup
    {
        public int Id { get; set; }
        public int InventoryId { get; set; }
        public Inventory Inventory { get; set; }
        public string Name { get; set; }

        public List<InventoryGood> InventoryGoods { get; set; }
    }

    public class InventoryGood
    {
        public int Id { get; set; }
        public int InventoryGroupId { get; set; }
        public InventoryGroup InventoryGroup { get; set; }
        public int GoodId { get; set; }
        public Good Good { get; set; }
        public double CountDB { get; set; } = 0;
        public double CountFact { get; set; } = 0;
        public double CountAppend { get; set; } = 0;
        public decimal Price { get; set; } 
    }
}
