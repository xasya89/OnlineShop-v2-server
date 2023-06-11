using Dapper;
using MySql.Data.MySqlClient;
using OnlineShop2.LegacyDb.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShop2.LegacyDb.Repositories
{
    public interface IShiftRepositoryLegacy
    {
        void SetConnectionString(string connectionString);
        public Task<IReadOnlyCollection<ShiftLegacy>> GetShifts(DateOnly with);
        public Task<IReadOnlyCollection<ShiftLegacy>> GetNewStartedShifts();
        public Task<IReadOnlyCollection<ShiftLegacy>> GetNewStoppedShifts();
        public Task<IReadOnlyCollection<CheckSellLegacy>> GetNewChecks();
        public Task SetProcessedComplite();
    }
    public class ShiftRepositoryLegacy : IShiftRepositoryLegacy
    {
        private string _connectionString;
        public void SetConnectionString(string connectionString) => _connectionString = connectionString;
        public List<int> documentHistoryIds = new();

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

        public async Task<IReadOnlyCollection<ShiftLegacy>> GetNewStartedShifts()
        {
            using var con = new MySqlConnection(_connectionString);
            con.Open();
            var shifts = await con.QueryAsync<ShiftLegacy>("SELECT s.* FROM shifts s INNER JOIN " +
                "(SELECT DocumentId FROM documenthistories WHERE DocumentType=0 AND Processed=0) as d " +
                "ON s.id=d.DocumentId");

            var ids = await con.QueryAsync<int>("SELECT id FROM documenthistories WHERE DocumentType=0 AND Processed=0");
            documentHistoryIds.AddRange(ids);

            return shifts.ToImmutableList();
        }

        public async Task<IReadOnlyCollection<ShiftLegacy>> GetNewStoppedShifts()
        {
            using var con = new MySqlConnection(_connectionString);
            con.Open();
            var shifts = await con.QueryAsync<ShiftLegacy>("SELECT s.* FROM shifts s INNER JOIN " +
                "(SELECT DocumentId FROM documenthistories WHERE DocumentType=1 AND Processed=0) as d " +
                "ON s.id=d.DocumentId");

            var ids = await con.QueryAsync<int>("SELECT id FROM documenthistories WHERE DocumentType=1 AND Processed=0");
            documentHistoryIds.AddRange(ids);

            return shifts.ToImmutableList();
        }

        public async Task<IReadOnlyCollection<CheckSellLegacy>> GetNewChecks()
        {
            using var con = new MySqlConnection(_connectionString);
            con.Open();
            var checks = await con.QueryAsync<CheckSellLegacy>("SELECT c.* FROM checksells c INNER JOIN " +
                "(SELECT DocumentId FROM documenthistories WHERE DocumentType=2 AND Processed=0) as d " +
                "ON c.id=d.DocumentId");
            var ids = checks.Select(c => c.Id).ToList();
            var checkGoods = await con.QueryAsync<CheckGoodLegacy>("SELECT * FROM checkgoods WHERE CheckSellId IN @Ids",
                new { Ids = ids });
            foreach(var check in checks)
                check.CheckGoods = checkGoods.Where(c=>c.CheckSellId == check.Id).ToList();

            var docIds = await con.QueryAsync<int>("SELECT id FROM documenthistories WHERE DocumentType=2 AND Processed=0");
            documentHistoryIds.AddRange(docIds);

            return checks.ToImmutableList();
        }

        public async Task SetProcessedComplite()
        {
            using var con = new MySqlConnection(_connectionString);
            con.Open();
            await con.ExecuteAsync("UPDATE documenthistories SET Processed=1 WHERE id IN @Ids", new { Ids = documentHistoryIds });
            documentHistoryIds.Clear();
        }
    }
}
