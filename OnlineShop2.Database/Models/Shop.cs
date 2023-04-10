using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShop2.Database.Models
{
    public class Shop
    {
        public int Id { get; set; }
        public string Alias { get; set; }
        public string OrgName { get; set; }
        public string Inn { get; set; }
        public string? Kpp { get; set; }
        public string Adress { get; set; }
        public int? LegacyDbNum { get; set; }

        public List<Inventory> Inventories { get; set; }
        public List<GoodCurrentBalance> GoodCurrentBalances { get; set; }
        public ICollection<Shift> Shifts { get; set; }
        public ICollection<InventoryAppendCheck> InventoryAppendChecks { get; set; }
        public ICollection<Arrival> Arrivals { get; set; }
    }
}
