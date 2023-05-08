using OnlineShop2.Database.Models;

namespace OnlineShop2.Api.Models.Arrival
{
    public class ArrivalModel
    {
        public int Id { get; set; }
        public int ShopId { get; set; }
        public DocumentStatus Status { get; set; }
        public string Num { get; set; }
        public DateTime DateArrival { get; set; }
        public int SupplierId { get; set; }
        public int? LegacyId { get; set; }
        public List<ArrivalGoodModel> ArrivalGoods { get; set; } = new();
    }

    public class ArrivalGoodModel
    {
        public int Id { get; set; }
        public int SequenceNum { get; set; }
        public int GoodId { get; set; }
        public decimal Count { get; set; }
        public decimal PricePurchase { get; set; }
        public NDSType Nds { get; set; }
        public decimal PriceSell { get; set; }
        public DateTime? ExpiresDate { get; set; }
    }
}
