using MySql.Data.MySqlClient;
using Org.BouncyCastle.Crypto.Tls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace OnlineShop2.LegacyDb.Infrastructure
{
    internal static class CurrentBalanceChange
    {
        public static async Task ChangeBalance(MySqlConnection con, Dictionary<int, decimal> balances)
        {
            foreach (var balance in balances)
                await con.ExecuteAsync("UPDATE goodcountbalancecurrents SET Count=Count+@Count WHERE GoodId=@GoodId", new
                {
                    GoodId = balance.Key,
                    Count = balance.Value
                });
        }
    }
}
