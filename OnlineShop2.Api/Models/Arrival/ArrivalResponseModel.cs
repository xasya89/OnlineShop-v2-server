using OnlineShop2.Database.Models;

namespace OnlineShop2.Api.Models.Arrival
{
    public class ArrivalResponseModel
    {
        public int Id { get; set; }
        public DocumentStatus Status { get; set; } = DocumentStatus.New;
        public string Num { get; set; }
        public DateTime DateArrival { get; set; } = DateTime.Now;
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public int ShopId { get; set; }
        public decimal PurchaseAmount { get; set; }
        public decimal SumNds { get; set; }
        public decimal SaleAmount { get; set; }
        public int? LegacyId { get; set; }
        public List<ArrivalGoodResponseModel> Positions { get; set; } = new();
    }

    public class ArrivalGoodResponseModel
    {
        public int Id { get; set; }
        public int GoodId { get; set; }
        public string GoodName { get; set; }
        public decimal Count { get; set; }
        public decimal PricePurchase { get; set; }
        public NDSType Nds { get; set; }
        public decimal PriceSell { get; set; }
        public DateTime? ExpiresDate { get; set; }
    }
}
