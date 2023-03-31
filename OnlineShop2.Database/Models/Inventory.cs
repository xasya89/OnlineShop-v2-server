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
        public List<InventorySummaryGood> InventorySummaryGoods { get; set; }
        public List<InventoryAppendCheck> InventoryAppendChecks { get; set; }
    }

    public class InventoryGroup
    {
        public int Id { get; set; }
        public int InventoryId { get; set; }
        public Inventory Inventory { get; set; }
        public string Name { get; set; }

        public List<InventoryGood> InventoryGoods { get; set; }
        public List<InventorySummaryGood> InventorySummaryGoods { get; set; }
        public List<InventoryAppendCheck> InventoryAppendChecks { get; set; }
    }

    public class InventoryGood
    {
        public int Id { get; set; }
        public int InventoryGroupId { get; set; }
        public InventoryGroup InventoryGroup { get; set; }
        public int GoodId { get; set; }
        public Good Good { get; set; }
        public decimal CountDB { get; set; } = 0;
        public decimal? CountFact { get; set; } 
        public decimal? CountAppend { get; set; }
        public decimal Price { get; set; } 
    }

    public class InventorySummaryGood
    {
        public int Id { get; set; }
        public int InventoryId { get; set; }
        public Inventory Inventory { get; set; }
        public int GoodId { get; set; }
        public Good Good { get; set; }
        public decimal Price { get; set; } = 0;
        public decimal CountOld { get; set; } = 0;
        public decimal CountCurrent { get; set; } = 0;
    }

    public class InventoryAppendCheck
    {
        public int Id { get; set; }
        public Inventory Inventory { get; set; }
        public int InventoryId { get; set; }
        public Shop Shop { get; set; }
        public int ShopId { get; set; }
        public CheckSell CheckSell { get; set; }
        public int CheckSellId { get; set; }
        public Good Good { get; set; }
        public int GoodId { get; set; }
        public decimal Count { get; set; }
    }
}
