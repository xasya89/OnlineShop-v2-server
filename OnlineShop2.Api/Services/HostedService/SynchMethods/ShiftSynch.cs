using Microsoft.EntityFrameworkCore;
using OnlineShop2.Database.Models;
using OnlineShop2.Database;
using OnlineShop2.LegacyDb.Repositories;
using OnlineShop2.Api.BizLogic;

namespace OnlineShop2.Api.Services.HostedService.SynchMethods
{
    internal class ShiftSynch
    {
        public static async Task StartSynch(OnlineShopContext context, IShiftRepositoryLegacy shiftRepository, int shopId, MoneyReportChannelService moneyReportChannelService)
        {
            using var tran = context.Database.BeginTransaction();
            try
            {
                var newShifts = await analyzeStartedShifts(context, shiftRepository, shopId, moneyReportChannelService);
                var stopedShifts = await analyzeStopedShifts(context, shiftRepository, shopId, moneyReportChannelService);
                var newChecks = await analyzeNewChecks(context, shiftRepository, shopId, moneyReportChannelService);

                await context.SaveChangesAsync();

                foreach (var check in newChecks)
                {
                    moneyReportChannelService.PushCheckMoney(check.DateCreate, shopId, check.SumNoElectron);
                    moneyReportChannelService.PushCheckMoney(check.DateCreate, shopId, check.SumElectron);
                }

                foreach (var shift in newShifts)
                    context.Entry<Shift>(shift).State = EntityState.Detached;
                foreach (var shift in stopedShifts)
                    context.Entry<Shift>(shift).State = EntityState.Detached;
                foreach (var check in newChecks)
                    context.Entry<CheckSell>(check).State = EntityState.Detached;
                foreach (var shiftSummary in context.ChangeTracker.Entries<ShiftSummary>())
                    shiftSummary.State = EntityState.Detached;

                await shiftRepository.SetProcessedComplite();

                tran.Commit();
            }
            catch(Exception ex)
            {
                tran.Rollback();
            }
        }

        private static async Task<IEnumerable<Shift>> analyzeStartedShifts(OnlineShopContext context, IShiftRepositoryLegacy shiftRepository, int shopId, MoneyReportChannelService moneyReportChannelService)
        {
            var newLegacyShifts = await shiftRepository.GetNewStartedShifts();
            var newShifts = newLegacyShifts.Select(legacy => new Shift
            {
                Start = legacy.Start,
                Stop = legacy.Stop,
                ShopId = shopId,
                Uuid = legacy.Uuid,
                SumAll = legacy.SumAll,
                SumElectron = legacy.SumElectron,
                SumNoElectron = legacy.SumNoElectron,
                SumSell = legacy.SumSell,
                SumDiscount = legacy.SumDiscount,
                SumReturnNoElectron = legacy.SumReturnNoElectron,
                SumReturnElectron = legacy.SumReturnElectron,
                LegacyId = legacy.Id,
            });
            context.Shifts.AddRange(newShifts);

            return newShifts;
        }

        private static async Task<IEnumerable<Shift>> analyzeStopedShifts(OnlineShopContext context, IShiftRepositoryLegacy shiftRepository, int shopId, MoneyReportChannelService moneyReportChannelService)
        {
            var stopedLegacyShifts = await shiftRepository.GetNewStoppedShifts();
            var stopedLegacyIds = stopedLegacyShifts.Select(x => x.Id).ToList();
            var shifts = await context.Shifts.Where(x => stopedLegacyIds.Contains(x.LegacyId ?? 0)).ToListAsync();
            foreach (var shift in shifts)
            {
                var legacy = stopedLegacyShifts.Where(x => x.Id == shift.LegacyId).First();
                shift.Stop = legacy.Stop;
                shift.SumAll = legacy.SumAll;
                shift.SumElectron = legacy.SumElectron;
                shift.SumNoElectron = legacy.SumNoElectron;
                shift.SumSell = legacy.SumSell;
                shift.SumDiscount = legacy.SumDiscount;
                shift.SumReturnNoElectron = legacy.SumReturnNoElectron;
                shift.SumReturnElectron = legacy.SumReturnElectron;
            }

            return shifts;
        }

        private static async Task<IEnumerable<CheckSell>> analyzeNewChecks(OnlineShopContext context, IShiftRepositoryLegacy shiftRepository, int shopId, MoneyReportChannelService moneyReportChannelService)
        {
            var newLegacyCHecks = await shiftRepository.GetNewChecks();
            if (newLegacyCHecks?.Count() == 0)
                return new List<CheckSell>();

            var shiftsLegacyIds = newLegacyCHecks.Select(x => x.ShiftId);
            var shifts = await context.Shifts.Where(x => shiftsLegacyIds.Contains(x.LegacyId ?? 0)).AsNoTracking().ToListAsync();
            var goodsLegacyIds = newLegacyCHecks.SelectMany(x => x.CheckGoods).Select(x => x.GoodId).ToList();
            var goods = await context.Goods.Where(g => goodsLegacyIds.Contains(g.LegacyId ?? 0)).AsNoTracking().ToListAsync();
            var newChecks = newLegacyCHecks.Select(c => new CheckSell
            {
                Id = 0,
                ShiftId = shifts.Where(x => x.LegacyId == c.ShiftId).FirstOrDefault()?.Id ?? 0,
                DateCreate = c.DateCreate,
                TypeSell = c.TypeSell,
                SumBuy = c.SumAll,
                SumDiscont = c.SumDiscont,
                SumElectron = c.SumElectron,
                SumNoElectron = c.SumCash,
                LegacyId = c.Id,
                CheckGoods = c.CheckGoods.Select(cg => new CheckGood
                {
                    GoodId = goods.Where(g => g.LegacyId == cg.GoodId).FirstOrDefault()?.Id ?? 0,
                    Count = cg.Count,
                    Price = cg.Price
                }).ToList()
            }).ToList();

            context.CheckSells.AddRange(newChecks);

            var checksSummary = newChecks.GroupBy(c => c.ShiftId)
                .Select(c => new {
                    ShiftId = c.Key,
                    Summaries = c.SelectMany(c => c.CheckGoods).GroupBy(c=>c.GoodId)
                        .Select(c => new { GoodId = c.Key, Sum = c.Sum(x=>x.Price * x.Count), Count = c.Sum(x => x.Count) })
                });
            foreach (var summary in checksSummary)
                foreach (var pos in summary.Summaries)
                {
                    var dbSummary = await context.ShiftSummaries.Where(x => x.ShiftId == summary.ShiftId & x.GoodId == pos.GoodId)
                        .AsNoTracking().FirstOrDefaultAsync();
                    if (dbSummary == null)
                        context.ShiftSummaries.Add(new ShiftSummary
                        {
                            ShiftId = summary.ShiftId,
                            GoodId = pos.GoodId,
                            Count = pos.Count,
                            Sum = pos.Sum
                        });
                    else
                    {
                        dbSummary.Count += pos.Count;
                        dbSummary.Sum += pos.Sum;
                    }
                };

            var balance = newChecks.SelectMany(x => x.CheckGoods).GroupBy(x => x.GoodId)
                .ToDictionary(x => x.Key, x => x.Sum(x => x.Count));
            await CurrentBalanceChange.Change(context, shopId, balance);

            context.CheckSells.AddRange(newChecks);

            return newChecks;
        }
    }
}
