using Dapper;
using MySql.Data.MySqlClient;
using OnlineShop2.Database.Models;
using OnlineShop2.LegacyDb.Infrastructure.MapperConfigurations;
using OnlineShop2.LegacyDb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShop2.LegacyDb.Repositories
{
    public class GoodCurrentBalanceLegacyRepository
    {
        private readonly MySqlConnection _connection;
        public GoodCurrentBalanceLegacyRepository(MySqlConnection connection) => _connection = connection;

        public async Task<IEnumerable<GoodCurrentBalance>> GetCurrent() =>
            MapperInstance.GetMapper().Map<IEnumerable<GoodCountBalanceCurrentLegacy>, IEnumerable<GoodCurrentBalance>>(await _connection.QueryAsync<GoodCountBalanceCurrentLegacy>("SELECT * FROM goodcountbalancecurrents"));

        public async Task SetCurrent(IEnumerable<GoodCurrentBalance> balance)
        {
            var tr = _connection.BeginTransaction();
            foreach (var item in balance)
                await _connection.ExecuteAsync($"UPDATE goodcountbalancecurrents SET Count={item.CurrentCount} WHERE GoodId={item.GoodId}");
            await _connection.ExecuteAsync("UPDATE goodcountbalancecurrents c INNER JOIN goods g ON c.goodId=g.id SET c.Count=0 WHERE g.IsDeleted=1");
            await tr.CommitAsync();
        }

        public async Task<bool> ShiftClosedStatus() =>
            (await _connection.QuerySingleAsync<int>("SELECT COUNT(*) FROM shifts WHERE stop IS NULL")) == 0;
    }
}
