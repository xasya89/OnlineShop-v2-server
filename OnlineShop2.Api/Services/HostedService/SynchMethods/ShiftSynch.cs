using Microsoft.EntityFrameworkCore;
using OnlineShop2.Database.Models;
using OnlineShop2.Database;
using OnlineShop2.LegacyDb.Repositories;
using OnlineShop2.Api.BizLogic;
using OnlineShop2.Api.Extensions;

namespace OnlineShop2.Api.Services.HostedService.SynchMethods
{
    internal class ShiftSynch
    {
        public static async Task StartSynch(OnlineShopContext context, 
            IShiftRepositoryLegacy shiftRepository, 
            int shopId, 
            MoneyReportChannelService moneyReportChannelService,
            ILogger logger)
        {
            using var tran = context.Database.BeginTransaction();
            try
            {
                //Новые чеки
                var newChecks = await analyzeNewChecks(context, shiftRepository, shopId, moneyReportChannelService);
                if (newChecks.Count() > 0)
                {
                    context.CheckSells.AddRange(newChecks);

                    await changeShiftSummary(context, newChecks);
                    await changeBalanceGoods(context, newChecks, shopId);

                    await context.SaveChangesAsync();
                    await shiftRepository.SetProcessedComplite();
                    await tran.CommitAsync();

                    foreach (var check in newChecks)
                    {
                        if (check.SumElectron > 0)
                            moneyReportChannelService.PushCheckElectron(check.DateCreate, shopId, check.Id, check.SumElectron);
                        if (check.SumNoElectron > 0)
                            moneyReportChannelService.PushCheckMoney(check.DateCreate, shopId, check.Id, check.SumNoElectron);
                    }

                    return;
                }

                //Закрытие смены
                var stopedShifts = await analyzeStopedShifts(context, shiftRepository, shopId, moneyReportChannelService);
                if (stopedShifts.Count() > 0)
                {
                    context.Shifts.UpdateRange(stopedShifts);
                    await context.SaveChangesAsync();
                    await shiftRepository.SetProcessedComplite();
                    tran.Commit();

                    foreach (var shift in stopedShifts)
                        moneyReportChannelService.PushCloseShift(shift.Start, shift.ShopId, shift.Id);

                    logger.LogInformation($"Синхронизировано {stopedShifts.Count()} закртых смен\n shopId {shopId}\nLegacyIds "
                        + string.Join(',', stopedShifts.Select(x => x.LegacyId)));
                    return;
                }

                //Открытие смены
                var newShifts = await analyzeStartedShifts(context, shiftRepository, shopId, moneyReportChannelService);
                if (newShifts.Count() > 0)
                {
                    context.Shifts.AddRange(newShifts);
                    await context.SaveChangesAsync();
                    await Task.Delay(200);
                    await shiftRepository.SetProcessedComplite();
                    await tran.CommitAsync();

                    var newShiftLegacyIds = newShifts.Select(x => x.LegacyId ?? 0);
                    var newShiftsDb = await context.Shifts.Where(s => newShiftLegacyIds.Contains(s.LegacyId ?? 0))
                        .AsNoTracking().ToListAsync();
                    newShiftsDb.ForEach(item =>
                        moneyReportChannelService.PushOpenShift(item.Start, item.ShopId, item.Id));
                    return;
                }

                


            }
            catch(Exception ex)
            {
                tran.Rollback();
                throw new Exception(nameof(ShiftSynch), ex);
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

            

            return newShifts;
        }

        private static async Task<IEnumerable<Shift>> analyzeStopedShifts(OnlineShopContext context, IShiftRepositoryLegacy shiftRepository, int shopId, MoneyReportChannelService moneyReportChannelService)
        {
            var stopedLegacyShifts = await shiftRepository.GetNewStoppedShifts();
            var stopedLegacyIds = stopedLegacyShifts.Select(x => x.Id).ToList();
            var shifts = await context.Shifts.Where(x => stopedLegacyIds.Contains(x.LegacyId ?? 0)).AsNoTracking().ToListAsync();
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
            var goodsLegacyIds = newLegacyCHecks.SelectMany(x => x.CheckGoods).Select(x => x.GoodId).ToList();
            var goods = await context.Goods.Where(g => goodsLegacyIds.Contains(g.LegacyId ?? 0)).AsNoTracking().ToListAsync();
            var checksResult = new List<CheckSell>();
            foreach(var item in newLegacyCHecks)
            {
                var shiftDb = await context.Shifts.Where(s => s.LegacyId == item.ShiftId).FirstOrDefaultAsync();
                if (shiftDb == null)
                    continue;
                var newCheckSell = new CheckSell
                {
                    Id = 0,
                    Shift = shiftDb,
                    DateCreate = item.DateCreate,
                    TypeSell = item.TypeSell,
                    SumBuy = item.SumAll,
                    SumDiscont = item.SumDiscont,
                    SumElectron = item.SumElectron,
                    SumNoElectron = item.SumCash,
                    LegacyId = item.Id,
                    CheckGoods = item.CheckGoods.Select(cg => new CheckGood
                    {
                        GoodId = goods.Where(g => g.LegacyId == cg.GoodId).First().Id,
                        Count = cg.Count,
                        Price = cg.Price
                    }).ToList()
                };
                context.CheckSells.Add(newCheckSell);
                shiftDb.SumElectron += item.SumElectron;
                shiftDb.SumNoElectron += item.SumCash;
                shiftDb.SumAll += item.SumElectron + item.SumCash;

                checksResult.Add(newCheckSell);
            }
            return checksResult;
        }

        /// <summary>
        /// Добавление в ShiftSummary новых строк с продажей (по товарам)
        /// </summary>
        /// <param name="context"></param>
        /// <param name="newChecksSell"></param>
        /// <returns></returns>
        // TODO: При создании новой строки Summary сравнивать цену продажи. Т.к. цена товара может измениться в течении дня
        private static async Task changeShiftSummary(OnlineShopContext context, IEnumerable<CheckSell> newChecksSell)
        {
            var checksGroupingShifts = newChecksSell.GroupBy(x => x.ShiftId);
            foreach(var checksShift in checksGroupingShifts)
            {
                var shiftId = checksShift.Key;
                var checks = checksShift.ToList();

                var summaryPrepare = checks.SelectMany(x=>x.CheckGoods).GroupBy(x => x.GoodId).Select(x => new { GoodId = x.Key, Count = x.Sum(x => x.Count) });
                foreach (var itemPrepare in summaryPrepare)
                {
                    var summary = await context.ShiftSummaries
                        .Where(s => s.ShiftId == shiftId & s.GoodId == itemPrepare.GoodId).FirstOrDefaultAsync();
                    
                    if (summary == null)
                    {
                        summary = new ShiftSummary
                        {
                            ShiftId = shiftId,
                            GoodId = itemPrepare.GoodId,
                            Count=itemPrepare.Count
                        };
                        context.ShiftSummaries.Add(summary);
                    }
                    summary.Count += itemPrepare.Count;
                }
            }
        }

        private static async Task changeBalanceGoods(OnlineShopContext context, IEnumerable<CheckSell> newChecksSell, int shopId)
        {
            var balance = newChecksSell.SelectMany(x => x.CheckGoods).GroupBy(x => x.GoodId)
                .ToDictionary(x => x.Key, x => x.Sum(x => x.Count));
            await CurrentBalanceChange.Change(context, shopId, balance);
        }

    }
}
