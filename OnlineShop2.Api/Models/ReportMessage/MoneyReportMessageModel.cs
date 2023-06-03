namespace OnlineShop2.Api.Models.ReportMessage
{
    public record MoneyReportMessageModel(MoneyReportMessageTypeDoc TypeDoc, DateTime Date, int ShopId, int DocId, decimal? Sum=null);

    public enum MoneyReportMessageTypeDoc
    {
        InventoryComplite,
        Arrival,
        StopShift,
        CheckMoney,
        CheckElectron,
        WriteOf,
        CashIncome,
        CashOutcome,
        RevaluationOld,
        RevaluationNew
    }
}
