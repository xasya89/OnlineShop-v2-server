using Microsoft.EntityFrameworkCore;
using OnlineShop2.Database.Models;
using OnlineShop2.Database;
using OnlineShop2.LegacyDb.Repositories;

namespace OnlineShop2.Api.Services.HostedService.SynchMethods
{
    internal class ShiftSynch
    {
        public static async Task StartSynch(OnlineShopContext context, IUnitOfWorkLegacy unitOfWork, int shopId, MoneyReportChannelService moneyReportChannelService)
        {
            DateOnly with = DateOnly.FromDateTime(DateTime.Now.AddDays(-1));
            var shiftsLegacy = await unitOfWork.ShiftRepository.GetShifts(with);
            IEnumerable<int?> shiftsLegacyId = Array.ConvertAll(shiftsLegacy.Select(s => s.Id).ToArray(), value => new int?(value));
            var shifts = await context.Shifts.Include(s => s.CheckSells).ThenInclude(c => c.CheckGoods).Include(s => s.ShiftSummaries)
                .Where(s => shiftsLegacyId.Contains(s.LegacyId)).AsNoTracking().ToListAsync();
            //Получаем список товаров в чеках 
            var goodsLegacyId = shiftsLegacy.SelectMany(s => s.CheckSells).SelectMany(c => c.CheckGoods).GroupBy(g => g.GoodId).Select(c => c.Key);
            IEnumerable<int?> goodsLegacyIdNullable = Array.ConvertAll(goodsLegacyId.ToArray(), val => new int?(val));
            var goods = await context.Goods.Where(g => goodsLegacyIdNullable.Contains(g.LegacyId)).AsNoTracking().ToListAsync();

            var newShifts = from legacy in shiftsLegacy
                            join shift in shifts on legacy.Id equals shift.LegacyId into t
                            from sub in t.DefaultIfEmpty()
                            where sub == null
                            select new Shift
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
                                CheckSells = legacy.CheckSells.Select(c => new CheckSell
                                {
                                    DateCreate = c.DateCreate,
                                    TypeSell = c.TypeSell,
                                    SumBuy = legacy.SumAll,
                                    SumDiscont = c.SumDiscont,
                                    SumElectron = c.SumElectron,
                                    SumNoElectron = c.SumCash,
                                    LegacyId = c.Id,
                                    CheckGoods = c.CheckGoods.Select(c => new CheckGood
                                    {
                                        GoodId = goods.Where(g => g.LegacyId == c.GoodId).First().Id,
                                        Count = c.Count,
                                        Price = c.Price
                                    }).ToList()
                                }).ToList()
                            };
            context.AddRange(newShifts);

            var editingShifts = from legacy in shiftsLegacy
                                join shift in shifts on legacy.Id equals shift.LegacyId into t
                                from sub in t.DefaultIfEmpty()
                                where sub != null
                                select new { db = sub, legacy = legacy };
            foreach (var row in editingShifts)
            {
                context.Entry(row.db).State = EntityState.Unchanged;
                var prop = context.Entry(row.db).Property(x => x.Start);
                row.db.Stop = row.legacy.Stop;
                row.db.SumAll = row.legacy.SumAll;
                row.db.SumElectron = row.legacy.SumElectron;
                row.db.SumNoElectron = row.legacy.SumNoElectron;
                row.db.SumSell = row.legacy.SumSell;
                row.db.SumDiscount = row.legacy.SumDiscount;
                row.db.SumReturnNoElectron = row.legacy.SumReturnNoElectron;
                row.db.SumReturnElectron = row.legacy.SumReturnElectron;
            }

            //Получим новые чеки
            var checksLegacy = shiftsLegacy.Where(s => !newShifts.Any(x => x.LegacyId == s.Id)).SelectMany(s => s.CheckSells);
            var checks = shifts.SelectMany(s => s.CheckSells).ToList();
            var newChecks = from legacy in checksLegacy
                            join check in checks on legacy.Id equals check.LegacyId into t
                            from sub in t.DefaultIfEmpty()
                            where sub == null
                            select new CheckSell
                            {
                                Shift = shifts.Where(s => s.LegacyId == legacy.ShiftId).FirstOrDefault() ?? newShifts.Where(s => s.LegacyId == legacy.ShiftId).First(),
                                DateCreate = legacy.DateCreate,
                                TypeSell = legacy.TypeSell,
                                SumBuy = legacy.SumAll,
                                SumDiscont = legacy.SumDiscont,
                                SumElectron = legacy.SumElectron,
                                SumNoElectron = legacy.SumCash,
                                CheckGoods = legacy.CheckGoods.Select(c => new CheckGood
                                {
                                    GoodId = goods.Where(g => g.LegacyId == c.GoodId).First().Id,
                                    Count = c.Count,
                                    Price = c.Price
                                }).ToList(),
                                LegacyId = legacy.Id
                            };
            context.AddRange(newChecks);

            foreach (var check in newChecks)
            {
                moneyReportChannelService.PushCheckMoney(check.DateCreate, shopId, check.SumNoElectron);
                moneyReportChannelService.PushCheckMoney(check.DateCreate, shopId, check.SumElectron);
            }

            await context.SaveChangesAsync();

            foreach (var shift in newShifts)
                context.Entry(shift).State = EntityState.Detached;
            foreach (var check in newChecks)
                context.Entry(check).State = EntityState.Detached;
        }
    }
}
