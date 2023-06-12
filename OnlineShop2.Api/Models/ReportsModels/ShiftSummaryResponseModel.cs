namespace OnlineShop2.Api.Models.ReportsModels
{
    public class ShiftResponseModel
    {
        public int Id { get; set; }
        public DateTime Start { get; set; }
        public DateTime? Stop { get; set; }
        public decimal SumAll { get; set; }
        public decimal SumNoElectron { get; set; }
        public decimal SumElectron { get; set; }
        public decimal SumSell { get; set; }
        public decimal SumDiscount { get; set; }
        public decimal SumReturnNoElectron { get; set; }
        public decimal SumReturnElectron { get; set; }
        public decimal SumIncome { get; set; }
        public decimal SumOutcome { get; set; }
        public string? CashierName { get; set; }
    }
}
