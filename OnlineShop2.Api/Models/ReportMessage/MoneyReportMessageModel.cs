using OnlineShop2.Dao;

namespace OnlineShop2.Api.Models.ReportMessage
{
    public record MoneyReportMessageModel(MoneyReportMessageTypeDoc TypeDoc, DateTime Date, int ShopId, int DocId, decimal? Sum=null);
}
