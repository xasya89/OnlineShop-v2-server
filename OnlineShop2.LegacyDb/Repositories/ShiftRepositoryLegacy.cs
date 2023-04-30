using Dapper;
using MySql.Data.MySqlClient;
using OnlineShop2.LegacyDb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShop2.LegacyDb.Repositories
{
    public class ShiftRepositoryLegacy : IShiftRepositoryLegacy
    {
        private string _connectionString;
        public void SetConnectionString(string connectionString) => _connectionString = connectionString;

        public async Task<IReadOnlyCollection<ShiftLegacy>> GetShifts(DateOnly with)
        {
            var withDate = with.ToDateTime(TimeOnly.MinValue);

            using (var con = new MySqlConnection(_connectionString))
            {
                con.Open();
                var shopId = await con.QueryFirstAsync<int>("SELECT id FROM shops");
                var shifts = await con.QueryAsync<ShiftLegacy>("SELECT * FROM shifts WHERE ShopId=@ShopId AND Start>=@With", new
                {
                    ShopId = shopId,
                    With = withDate
                });
                var shiftsId = shifts.Select(s => s.Id).ToArray();
                var checkSells = await con.QueryAsync<CheckSellLegacy>("SELECT * FROM checksells WHERE ShiftId IN @ShiftsId", new
                {
                    ShiftsId=shiftsId
                });
                int[] checkSellsId = checkSells.Select(s => s.Id).ToArray();
                
                foreach (var checkSell in checkSells)
                    shifts.Where(s => s.Id == checkSell.ShiftId).First().CheckSells.Add(checkSell);
                
                var checkGoods = await con.QueryAsync<CheckGoodLegacy>("SELECT * FROM checkgoods WHERE CheckSellId IN @CheckSellsId", new
                {
                    CheckSellsId = checkSellsId
                });
                foreach (var checkGood in checkGoods)
                    checkSells.Where(c => c.Id == checkGood.CheckSellId).First().CheckGoods.Add(checkGood);
                
                return shifts.ToList();
            }
        }

    }
}
