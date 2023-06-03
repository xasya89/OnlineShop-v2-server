using Microsoft.EntityFrameworkCore;
using OnlineShop2.Database;
using OnlineShop2.Database.Models;

namespace OnlineShop2.Api.Services.HostedService.MoneyReportMesssage.BixLogic
{
    internal class FindMoneyReport
    {
        public static async Task<MoneyReport> GetReport(OnlineShopContext context, DateTime date, int shopId)
        {
            var dateWithoutTime = DateOnly.FromDateTime(date).ToDateTime(TimeOnly.MinValue);
            var report = await context.MoneyReports
                .Where(x => DateTime.Compare(x.Create, dateWithoutTime) == 0 & x.ShopId==shopId)
                .FirstOrDefaultAsync();
            if (report != null)
                return report;
            report = new MoneyReport { Create = dateWithoutTime, ShopId = shopId };
            context.MoneyReports.Add(report);
            return report;
        }
    }
}
