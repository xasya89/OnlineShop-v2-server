using OnlineShop2.Dao;
using OnlineShop2.Database.Models;
using OnlineShop2.Dao.Extensions;

namespace OnlineShop2.Api.Models.Writeof
{
    public class WriteofModel
    {
        public int Id { get; set; }
        public DocumentStatus Status { get; set; }
        public DateTime DateWriteof { get; set; }
        public int ShopId { get; set; }
        public string Note { get; set; }
        public decimal SumAll { get; set; }

        public List<WriteofGoodModel> WriteofGoods { get; set; } = new();
    }

    public class WriteofGoodModel
    {
        public int Id { get; set; }
        public int GoodId { get; set; }
        public Good Good { private get; set; }
        public string? GoodName { get => Good?.Name; }
        public Units? Unit { get => Good?.Unit; }
        public string? UnitStr { get => Unit?.GetDisplayName(); }
        public decimal Price { get; set; }
        public decimal Count { get; set; }
    }
}
