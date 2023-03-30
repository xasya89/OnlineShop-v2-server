using MySql.Data.MySqlClient;
using OnlineShop2.Database.Models;
using OnlineShop2.LegacyDb.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using OnlineShop2.Database;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace OnlineShop2.LegacyDb.Repositories
{
    public class ShiftLegacyRepository
    {

        private readonly MySqlConnection _connection;
        public ShiftLegacyRepository(MySqlConnection connection) => _connection = connection;

        public async Task<IEnumerable<ShiftLegacy>> GetShifts(DateOnly with)
        {
            if (_connection.State != System.Data.ConnectionState.Open)
                _connection.Open();
            DateTime withDateTime = with.ToDateTime(TimeOnly.MinValue);
            var shifts = await _connection.QueryAsync<ShiftLegacy>("SELECT * FROM Shifts WHERE start>=@Start OR stop>=@Start", new { Start = withDateTime });
            var shiftsId = shifts.Select(s => s.Id);
            var checks = await _connection.QueryAsync<CheckSellLegacy>("SELECT * FROM checksells WHERE ShiftId IN @ShiftsId", new { ShiftsId = shiftsId });
            var checksId = checks.Select(c => c.Id);
            var checkGoods = await _connection.QueryAsync<CheckGoodLegacy>("SELECT * FROM checkgoods WHERE CheckSellId IN @CheckSellId", new { CheckSellId = checksId });
            var shiftsales = await _connection.QueryAsync<ShiftSaleLegacy>("SELECT * FROM shiftsales WHERE ShiftId IN @ShiftId", new { ShiftId = shiftsId });
            foreach(var shift in shifts)
            {
                foreach (var check in checks.Where(c => c.ShiftId == shift.Id))
                    check.CheckGoods = checkGoods.Where(c => c.CheckSellId == check.Id).ToList();

                shift.CheckSells = checks.Where(c => c.ShiftId == shift.Id).ToList();
                shift.ShiftSales = shiftsales.Where(s => s.ShiftId == shift.Id).ToList();
            };

            return shifts;
        }

        public async Task ShiftSynch(OnlineShopContext context, DateOnly with, int shopId)
        {
            var shiftsLegacy = await GetShifts(with);
            IEnumerable<int?> shiftsLegacyId = Array.ConvertAll(shiftsLegacy.Select(s => s.Id).ToArray(), value=>new int?(value));
            var shifts = await context.Shifts.Include(s=>s.CheckSells).ThenInclude(c=>c.CheckGoods).Include(s=>s.ShiftSummaries)
                .Where(s=> shiftsLegacyId.Contains(s.LegacyId)).ToListAsync();
            var goodsLegacyId = shiftsLegacy.SelectMany(s => s.CheckSells).SelectMany(c => c.CheckGoods).GroupBy(g=>g.GoodId).Select(c => c.Key);
            IEnumerable<int?> goodsLegacyIdNullable = Array.ConvertAll(goodsLegacyId.ToArray(), val => new int?(val));
            var goods = await context.Goods.Where(g => goodsLegacyIdNullable.Contains(g.LegacyId)).ToListAsync();
            var newShifts = from legacy in shiftsLegacy
                            join shift in shifts on legacy.Id equals shift.LegacyId into t
                            from sub in t.DefaultIfEmpty()
                            where sub == null
                            select new Shift
                            {
                                Start= legacy.Start,
                                Stop = legacy.Stop,
                                ShopId=shopId,
                                Uuid=legacy.Uuid,
                                SumAll = legacy.SumAll,
                                SumElectron = legacy.SumElectron,
                                SumNoElectron = legacy.SumNoElectron,
                                SumSell=legacy.SumSell,
                                SumDiscount=legacy.SumDiscount,
                                SumReturnNoElectron=legacy.SumReturnNoElectron,
                                SumReturnElectron=legacy.SumReturnElectron,
                                LegacyId=legacy.Id
                            };
            context.AddRange(newShifts);

            var editingShifts = from legacy in shiftsLegacy
                                join shift in shifts on legacy.Id equals shift.LegacyId into t
                                from sub in t.DefaultIfEmpty()
                                where sub != null
                                select new { db = sub, legacy = legacy };
            foreach(var row in editingShifts)
            {
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
            var checksLegacy = shiftsLegacy.SelectMany(s => s.CheckSells);
            var checks = shifts.SelectMany(s => s.CheckSells).ToList();
            var newChecks = from legacy in checksLegacy
                            join check in checks on legacy.Id equals check.LegacyId into t
                            from sub in t.DefaultIfEmpty()
                            where sub == null
                            select new CheckSell
                            {
                                Shift = shifts.Where(s => s.LegacyId == legacy.ShiftId).First(),
                                DateCreate = legacy.DateCreate,
                                TypeSell = legacy.TypeSell,
                                SumBuy = legacy.SumAll,
                                SumDiscont = legacy.SumDiscont,
                                SumElectron = legacy.SumElectron,
                                SumNoElectron = legacy.SumCash,
                                CheckGoods = legacy.CheckGoods.Select(c => new CheckGood
                                {
                                    Good = goods.Where(g => g.LegacyId == c.GoodId).First(),
                                    Count = c.Count,
                                    Price = c.Price
                                }).ToList()
                            };
            context.AddRange(newChecks);
        }
    }
}
