using Microsoft.EntityFrameworkCore;
using OnlineShop2.Database;
using OnlineShop2.Database.Models;
using OnlineShop2.Dao;

namespace OnlineShop2.Api.Services.HostedService.MoneyReportMesssageHostService.BizLogic
{
    internal class FindMoneyReport
    {
        /// <summary>
        /// Получим отчет
        /// </summary>
        /// <param name="context"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static async Task<MoneyReport> GetReport(OnlineShopContext context, Models.ReportMessage.MoneyReportMessageModel message)
        {
            var dateWithoutTime = DateOnly.FromDateTime(message.Date).ToDateTime(TimeOnly.MinValue);
            if (message.TypeDoc == MoneyReportMessageTypeDoc.CheckMoney || message.TypeDoc == MoneyReportMessageTypeDoc.CheckElectron)
                dateWithoutTime = await getDateShiftFromCheck(context, message.DocId);
            if (message.TypeDoc == MoneyReportMessageTypeDoc.CloseShift)
                dateWithoutTime = await getDateShift(context, message.ShopId);
            

            var report = await context.MoneyReports
                .Where(x => DateTime.Compare(x.Create, dateWithoutTime) == 0 & x.ShopId==message.ShopId)
                .FirstOrDefaultAsync();
            if (report != null)
                return report;
            
            report = new MoneyReport { Create = dateWithoutTime, ShopId = message.ShopId };
            context.MoneyReports.Add(report);
            return report;
        }

        /// <summary>
        /// Получим дату открытия смены
        /// </summary>
        /// <param name="context"></param>
        /// <param name="checkId"></param>
        /// <returns></returns>
        private async static Task<DateTime> getDateShiftFromCheck(OnlineShopContext context, int checkId)
        {
            var check = await context.CheckSells.Where(x => x.Id == checkId).AsNoTracking().FirstAsync();
            var shift = await context.Shifts.Where(x => x.Id == check.ShiftId).AsNoTracking().FirstAsync();
            return DateOnly.FromDateTime(shift.Start).ToDateTime(TimeOnly.MinValue);
        }

        private async static Task<DateTime> getDateShift(OnlineShopContext context, int shiftId)
        {
            var shift = await context.Shifts.Where(x => x.Id == shiftId).FirstOrDefaultAsync();
            return DateOnly.FromDateTime(shift.Start).ToDateTime(TimeOnly.MinValue);
        }
    }
}
