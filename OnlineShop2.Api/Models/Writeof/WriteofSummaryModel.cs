using OnlineShop2.Database.Models;

namespace OnlineShop2.Api.Models.Writeof
{
    public class WriteofSummaryModel
    {
        public int Id { get; set; }
        public DocumentStatus Status { get; set; }
        public DateTime DateWriteof { get; set; }
        public string DateWriteofStr { get => DateWriteof.ToString("dd.MM.yy"); }
        public int ShopId { get; set; }
        public string Note { get; set; }
        public decimal SumAll { get; set; }
    }
}
