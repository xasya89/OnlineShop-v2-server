using OnlineShop2.Dao;
using OnlineShop2.Database.Models;

namespace OnlineShop2.Api.Models.Arrival
{
    public class ArrivalResponseModel
    {
        public int Id { get; set; }
        public DocumentStatus Status { get; set; }
        public string Num { get; set; }
        public DateTime DateArrival { get; set; }
        public int SupplierId { get; set; }
        public int ShopId { get; set; }
        public decimal PurchaseAmount { get; set; }
        public decimal SumNds { get; set; }
        public decimal SaleAmount { get; set; }
        public int? LegacyId { get; set; }
        public List<ArrivalGoodResponseModel> ArrivalGoods { get; set; } = new();
    }

    public class ArrivalGoodResponseModel
    {
        public int Id { get; set; }
        public int GoodId { get; set; }
        public string GoodName { get=>Good?.Name ?? ""; }
        public Units Unit { get => Good?.Unit ?? Units.PCE; }
        public Good Good { private get; set; }
        public decimal Count { get; set; }
        public decimal PricePurchase { get; set; }
        public NDSType Nds { get; set; }
        public decimal PriceSell { get; set; }
        public DateTime? ExpiresDate { get; set; }
    }
}
