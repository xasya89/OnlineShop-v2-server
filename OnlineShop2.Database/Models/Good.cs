using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;

namespace OnlineShop2.Database.Models
{
    [Index(nameof(LegacyId))]
    public class GoodGroup
    {
        public int Id { get; set; }
        public int ShopId { get; set; }
        public Shop Shop { get; set; }
        public string Name { get; set; }
        public int? LegacyId { get; set; }

        public List<Good> Goods { get; set; } = new();
    }

    [Index(nameof(LegacyId))]
    public class Supplier
    {
        public int Id { get; set; }
        public int ShopId { get; set; }
        public Shop Shop { get; set; }
        public string Name { get; set; }
        public int? LegacyId { get; set; }
        public List<Good> Goods { get; set; } = new();
    }

    [Index(nameof(LegacyId))]
    public class Good
    {
        public int Id { get; set; }
        public int GoodGroupId { get; set; }
        public GoodGroup GoodGroup { get; set; }
        public int? ShopId { get; set; }
        public Shop? Shop { get; set; }
        public int? SupplierId { get; set; }
        public Supplier? Supplier { get; set; }
        public string Name { get; set; }
        public string? Article { get; set; }
        public Units Unit { get; set; }
        public decimal Price { get; set; }
        public SpecialTypes SpecialType { get; set; } = SpecialTypes.None;
        public double? VPackage { get; set; }
        public bool IsDeleted { get; set; } = false;
        public Guid Uuid { get; set; } = Guid.NewGuid();
        public int? LegacyId { get; set; } = null;

        public GoodCurrentBalance? CurrentBalance { get; set; }

        public List<GoodPrice> GoodPrices { get; set; } = new();
        public List<Barcode> Barcodes { get; set; } = new();

        public List<InventoryGood> InventoryGoods { get; set; }

        public ICollection<CheckGood> CheckGoods { get; set; }
        public ICollection<ShiftSummary> ShiftSummaries { get; set; }
    }

    public class GoodPrice
    {
        public int Id { get; set; }
        public Shop Shop { get; set; }
        public int ShopId { get; set; }
        public decimal Price { get; set; }
    }
    public class Barcode
    {
        public int Id { get; set; }
        public int GoodId { get; set; }
        public Good Good { get; set; }
        public string Code { get; set; }
    }

    public enum Units
    {
        [Display(Name = "шт")]
        [Description("шт")]
        PCE = 796, //штука
        [Display(Name = "л")]
        [Description("л")]
        Litr = 112, //литр
        [Display(Name = "кг")]
        [Description("кг")]
        KG = 166, //килограмм
        /*
        [Display(Name = "м")]
        MTR =6, //метр
        [Display(Name = "уп")]
        NMP =778, //упаковка
        [Display(Name = "см")]
        CMT =4 //сантиметр
        */
    }

    public enum SpecialTypes
    {
        [Display(Name = "")]
        None,
        [Display(Name = "Пиво")]
        Beer,
        [Display(Name = "Тара")]
        Bottle,
        [Display(Name = "Пакет")]
        Bag
    }
}
