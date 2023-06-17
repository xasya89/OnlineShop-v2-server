using Microsoft.EntityFrameworkCore;
using OnlineShop2.Database;
using OnlineShop2.LegacyDb.Repositories;
using OnlineShop2.Database.Models;
using AutoMapper;

namespace OnlineShop2.Api.BizLogic.InitialLogic
{
    public class InitialMoneyReports
    {
        public static async Task StartInitial(IServiceProvider serviceProvider)
        {
            using(var scope = serviceProvider.CreateScope())
            {
                using var context = scope.ServiceProvider.GetRequiredService<OnlineShopContext>();
                var unitOfWOrkLegacy = scope.ServiceProvider.GetRequiredService<IUnitOfWorkLegacy>();
                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var shops = await context.Shops.ToListAsync();
                var lastDateTime = DateOnly.FromDateTime(DateTime.Now.AddDays(-1)).ToDateTime(TimeOnly.MinValue);
                var currentDatetime = DateOnly.FromDateTime(DateTime.Now.AddDays(-1)).ToDateTime(TimeOnly.MinValue);
                foreach (var shop in shops)
                {
                    if (await context.MoneyReports.Where(x => x.ShopId == shop.Id).AnyAsync())
                        continue;

                    var lastDay = new MoneyReport
                    {
                        Create = currentDatetime,
                        ShopId = shop.Id
                    };
                    var currentReport = new MoneyReport
                    {
                        Create = currentDatetime,
                        ShopId = shop.Id
                    };

                    if (shop.LegacyDbNum is not null)
                    {
                        string connectionString = configuration.GetConnectionString("shop" + shop.LegacyDbNum);
                        unitOfWOrkLegacy.SetConnectionString(connectionString);
                        lastDay = mapper.Map<MoneyReport>(await unitOfWOrkLegacy.MoneyReportRepositoryLegacy.Get(lastDateTime));
                        lastDay.ShopId = shop.Id;
                        
                        currentReport = mapper.Map<MoneyReport>(await unitOfWOrkLegacy.MoneyReportRepositoryLegacy.Get(currentDatetime));
                        currentReport.ShopId = shop.Id;

                        //Если сегодня еще не была открыта смена, то то рассчитаем на начало дня
                        var shifts = await unitOfWOrkLegacy.ShiftRepository.GetShifts(DateOnly.FromDateTime(currentDatetime));
                        if (!shifts.Where(x => x.Start >= currentDatetime).Any())
                        {
                            lastDay.StopGoodSum = await unitOfWOrkLegacy.CurrentBalance.GetAllSum();
                            currentReport.StartGoodSum = await unitOfWOrkLegacy.CurrentBalance.GetAllSum();
                        }
                    };

                    context.MoneyReports.Add(lastDay);
                    context.MoneyReports.Add(currentReport);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
