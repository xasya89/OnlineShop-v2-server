using OnlineShop2.Dao;
using OnlineShop2.Dao.Extensions;
using OnlineShop2.Database.Models;

namespace OnlineShop2.Api.Models.ReportsModels
{
    public class ShiftSummaryResponse
    {
        public int Id { get; set; }
        public int GoodId { get; set; }
        public string GoodName { get; set; }
        public Units Unit { get; set; }
        public string UnitStr { get => Unit.GetDisplayName(); }
        public decimal Count { get; set; }
        public decimal Sum { get; set; }
        public decimal CountReturn { get; set; }
        public decimal SumReturn { get; set; }
    }
}
