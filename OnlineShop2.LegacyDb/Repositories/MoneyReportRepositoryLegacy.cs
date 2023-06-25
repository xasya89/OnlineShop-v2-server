using OnlineShop2.LegacyDb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using MySql.Data.MySqlClient;
using OnlineShop2.Dao;

namespace OnlineShop2.LegacyDb.Repositories
{
    public interface IMoneyReportRepositoryLegacy
    {
        public void SetConnectionString(string connectionString);
        public Task<MoneyReportLegacy> Get(DateTime currentDate);
    }
    public class MoneyReportRepositoryLegacy : IMoneyReportRepositoryLegacy
    {
        private string _connectionString;
        public async Task<MoneyReportLegacy> Get(DateTime currentDate)
        {
            currentDate = DateOnly.FromDateTime(currentDate).ToDateTime(TimeOnly.MinValue);
            var with = DateOnly.FromDateTime(currentDate).ToDateTime(TimeOnly.MinValue);
            var by = DateOnly.FromDateTime(currentDate.AddDays(1)).ToDateTime(TimeOnly.MinValue);
            using MySqlConnection con = new MySqlConnection(_connectionString);
            con.Open();
            var report = new MoneyReportLegacy { Create = currentDate};
            report.InventoryGoodsSum = await con.QueryFirstOrDefaultAsync<decimal>("SELECT s.SumFact FROM stocktakings s WHERE s.Create >= @With ", new { With = with });
            report.InventoryCashMoney = await con.QueryFirstOrDefaultAsync<decimal>("SELECT s.CashMoneyFact FROM stocktakings s WHERE s.Create >= @With ", new { With = with });
            report.ArrivalsSum = await con.QuerySingleAsync<decimal>("SELECT IFNULL(SUM(SumArrival),0) FROM arrivals WHERE DateArrival = @With", new { With = with, By = by });
            report.CashOutcome = await con.QuerySingleAsync<decimal>("SELECT IFNULL(SUM(c.Sum), 0) FROM cashmoneys c WHERE c.TypeOperation=1 AND c.Note NOT LIKE 'Смена %' AND c.Create BETWEEN @With AND @By", new { With = with, By = by });
            report.CashIncome = await con.QuerySingleAsync<decimal>("SELECT IFNULL(SUM(c.Sum), 0) FROM cashmoneys c WHERE c.TypeOperation=2 AND c.Note NOT LIKE 'Смена %' AND c.Create BETWEEN @With AND @By", new { With = with, By = by });
            report.CashElectron = await con.QuerySingleAsync<decimal>("SELECT IFNULL(SUM(SumElectron), 0) FROM shifts WHERE Start BETWEEN @With AND @By", new { With = with, By = by });
            report.CashMoney = await con.QuerySingleAsync<decimal>("SELECT IFNULL(SUM(SumNoElectron), 0) FROM shifts WHERE Start BETWEEN @With AND @By", new { With = with, By = by });
            report.Writeof = await con.QuerySingleAsync<decimal>("SELECT IFNULL(SUM(SumAll), 0) FROM writeofs WHERE DateWriteof = @With ", new { With = with });
            report.RevaluationOld = await con.QuerySingleAsync<decimal>("SELECT IFNULL(SUM(r.SumOld), 0) FROM revaluations r WHERE r.Create = @With ", new { With = with });
            report.RevaluationNew = await con.QuerySingleAsync<decimal>("SELECT IFNULL(SUM(r.SumNew), 0) FROM revaluations r WHERE r.Create = @With ", new { With = with });
            
            return report;
        }

        public void SetConnectionString(string connectionString) => _connectionString = connectionString;
    }
}
