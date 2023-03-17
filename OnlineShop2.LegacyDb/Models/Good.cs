using OnlineShop2.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OnlineShop2.LegacyDb.Models
{
    public class GoodGroupLegacy
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Good> Goods { get; set; }
    }

    public class SupplierLegacy
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Inn { get; set; }
        public List<Good> Goods { get; set; }
    }

    public class GoodLegacy
    {
        public int Id { get; set; }
        public int GoodGroupId { get; set; }
        public int? SupplierId { get; set; }
        public Guid Uuid { get; set; }
        public string Name { get; set; }
        public string Article { get; set; }
        public string BarCode { get; set; }
        public Units Unit { get; set; }
        public decimal Price { get; set; }
        public SpecialTypes SpecialType { get; set; } = SpecialTypes.None;
        public double? VPackage { get; set; }
        public bool IsDeleted { get; set; } = false;


        public List<BarCodeLegacy> Barcodes { get; set; } = new();
        public List<GoodPriceLegacy> GoodPrices { get; set; } = new();
    }
    public class BarCodeLegacy
    {
        public int Id { get; set; }
        public int GoodId { get; set; }
        public string Code { get; set; }
    }
    public class GoodPriceLegacy
    {
        public int Id { get; set; }
        public int GoodId { get; set; }
        public decimal Price { get; set; }
    }

}
