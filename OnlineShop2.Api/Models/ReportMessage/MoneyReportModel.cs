namespace OnlineShop2.Api.Models.ReportMessage
{
    public class MoneyReportResponseModel
    {
        public int Id { get; set; }
        public DateTime Create { get; set; }
        public string CreateStr { get => Create.ToString("dd.MM.yy"); }
        public decimal InventoryGoodsSum { get; set; }
        public decimal InventoryCashMoney { get; set; }
        public decimal ArrivalsSum { get; set; }
        public decimal CashIncome { get; set; }
        public decimal CashOutcome { get; set; }
        public decimal CashMoney { get; set; }
        public decimal CashElectron { get; set; }
        public decimal Writeof { get; set; }
        public decimal RevaluationOld { get; set; }
        public decimal RevaluationNew { get; set; }
        public decimal MoneyItog { get; set; }
        public decimal StopGoodSum { get; set; }
    }
}
