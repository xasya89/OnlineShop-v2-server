using OnlineShop2.Database.Models;

namespace OnlineShop2.Api.Models.Arrival
{
    public class ArrivalSummaryResponseModel
    {
        public int Id { get; set; }
        public DocumentStatus Status { get; set; } 
        public string Num { get; set; }
        public DateTime DateArrival { get; set; }
        public string DateArrivalStr { get => DateArrival.ToString("dd.MM.YY"); }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public int ShopId { get; set; }
        public decimal PurchaseAmount { get; set; }
        public decimal SumNds { get; set; }
        public decimal SaleAmount { get; set; }
        public int? LegacyId { get; set; }
    }
}
