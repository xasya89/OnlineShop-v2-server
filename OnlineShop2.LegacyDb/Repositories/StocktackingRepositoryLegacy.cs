using OnlineShop2.LegacyDb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Dapper;

namespace OnlineShop2.LegacyDb.Repositories
{
    public interface IStocktackingRepositoryLegacy
    {
        void SetConnectionString(string connectionString);
        Task<StocktackingSummaryLegacy?> GetNewStocktacking();
        Task SetCompliteProccessing();
    }
    public class StocktackingRepositoryLegacy : IStocktackingRepositoryLegacy
    {
        private string _connectionString;
        public async Task<StocktackingSummaryLegacy?> GetNewStocktacking()
        {
            using MySqlConnection con = new MySqlConnection(_connectionString);
            con.Open();
            return await con.QueryFirstOrDefaultAsync<StocktackingSummaryLegacy>("SELECT s.* FROM stocktakings s " +
                "INNER JOIN (SELECT DocumentId FROM documenthistories WHERE DocumentType=6 AND Processed=0) d " +
                "ON s.id=d.DocumentId ORDER BY s.Create DESC LIMIT 1");
        }

        public async Task SetCompliteProccessing()
        {
            using MySqlConnection con = new MySqlConnection(_connectionString);
            con.Open();
            await con.ExecuteAsync("UPDATE documenthistories SET Processed=1 WHERE Processed=0 AND DocumentType=6");
        }

        public void SetConnectionString(string connectionString) =>
            _connectionString = connectionString;
    }
}
