using Dapper;
using MySql.Data.MySqlClient;
using Mysqlx.Resultset;
using OnlineShop2.LegacyDb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShop2.LegacyDb.Repositories
{
    public interface ICurrentBalanceRepositoryLegacy
    {
        void SetConnectionString(string connectionString);
        Task<IEnumerable<GoodCountBalanceCurrentLegacy>> GetCurrent();
        Task SetCurrent(IEnumerable<GoodCountBalanceCurrentLegacy> balance);
        Task<bool> ShiftClosedStatus();
    }
    public class CurrentBalanceRepositoryLegacy:ICurrentBalanceRepositoryLegacy
    {
        private string _connectionString;
        public void SetConnectionString(string connectionString) => _connectionString = connectionString;

        public async Task<IEnumerable<GoodCountBalanceCurrentLegacy>> GetCurrent()
        {
            using (MySqlConnection con = new MySqlConnection(_connectionString))
            {
                con.Open();
                return await con.QueryAsync<GoodCountBalanceCurrentLegacy>("SELECT * FROM goodcountbalancecurrents");
            }
        }

        public async Task SetCurrent(IEnumerable<GoodCountBalanceCurrentLegacy> balance)
        {
            using(MySqlConnection con = new MySqlConnection(_connectionString))
            {
                con.Open();
                var tr = con.BeginTransaction();
                StringBuilder builder = new StringBuilder();
                foreach (var item in balance) 
                    builder.Append($"{(builder.Length > 0 ? "," : "VALUES")} ROW({item.GoodId}, {item.Count})");
                await con.ExecuteAsync($"UPDATE goodcountbalancecurrents b INNER JOIN ({builder}) t ON b.goodId=t.column_0 SET b.count=t.column_1");
                await con.ExecuteAsync("UPDATE goodcountbalancecurrents c INNER JOIN goods g ON c.goodId=g.id SET c.Count=0 WHERE g.IsDeleted=1");
                await tr.CommitAsync();
            }
        }

        public async Task<bool> ShiftClosedStatus()
        {
            using(MySqlConnection con = new MySqlConnection(_connectionString))
            {
                con.Open();
                return (await con.QuerySingleAsync<int>("SELECT COUNT(*) FROM shifts WHERE stop IS NULL")) == 0;
            }
        }

    }
}
