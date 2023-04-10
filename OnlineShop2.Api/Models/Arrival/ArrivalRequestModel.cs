using OnlineShop2.Database.Models;

namespace OnlineShop2.Api.Models.Arrival
{
    public class ArrivalRequestModel
    {
        public int Id { get; set; }
        public DocumentStatus Status { get; set; }
        public string Num { get; set; }
        public DateTime DateArrival { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public int ShopId { get; set; }
        public int? LegacyId { get; set; }
        public List<ArrivalGoodRequestModel> Positions { get; set; } = new();
    }

    public class ArrivalGoodRequestModel
    {
        public int Id { get; set; }
        public int SequenceNum { get; set; }
        public int GoodId { get; set; }
        public string GoodName { get; set; }
        public decimal Count { get; set; }
        public decimal PricePurchase { get; set; }
        public NDSType Nds { get; set; }
        public decimal PriceSell { get; set; }
        public DateTime? ExpiresDate { get; set; }
    }
}
