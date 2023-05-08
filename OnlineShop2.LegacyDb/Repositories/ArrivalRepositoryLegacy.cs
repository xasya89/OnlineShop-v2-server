using Dapper;
using Google.Protobuf.WellKnownTypes;
using MySql.Data.MySqlClient;
using OnlineShop2.LegacyDb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Dapper.SqlMapper;

namespace OnlineShop2.LegacyDb.Repositories
{
    public class ArrivalRepositoryLegacy : IArrivalRepositoryLegacy
    {
        private string _connectionString;
        public async Task<int> AddAsync(ArrivalLegacy entity)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var transaction = conn.BeginTransaction();
                try
                {
                    var id = await addAsync(transaction, entity);
                    await transaction.CommitAsync();
                    return id;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw ex;
                }
            }
        }

        public async Task<IReadOnlyCollection<ArrivalLegacy>> AddRangeAsync(IEnumerable<ArrivalLegacy> entities)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var transaction = conn.BeginTransaction();
                try
                {
                    foreach(var entity in entities)
                        entity.Id = await addAsync(transaction, entity);
                    await transaction.CommitAsync();
                    return entities.ToList();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw ex;
                }
            }
        }

        private async Task<int> addAsync(MySqlTransaction transaction, ArrivalLegacy entity)
        {
            var conn = transaction.Connection;
            var id = await conn.QuerySingleAsync<int>(@"INSERT INTO arrivals (Num, DateArrival, SupplierId, ShopId, isSuccess, SumPayments, SumArrival, SumNds, SumSell, Status) 
VALUES (@Num, @DateArrival, @SupplierId, @ShopId, @isSuccess, @SumPayments, @SumArrival, @SumNds, @SumSell, @Status); SELECT LAST_INSERT_ID()", new
            {
                Num = entity.Num,
                DateArrival=entity.DateArrival,
                SupplierId = entity.SupplierId,
                ShopId = entity.ShopId,
                isSuccess = 1,
                SumPayments = entity.SumPayments,
                SumArrival = entity.SumArrival,
                SumNds = entity.SumNds,
                SumSell = entity.SumSell,
                Status = (int)DocumentStatusLegacy.Confirm
            });
            foreach (var arrivalGood in entity.ArrivalGoods)
                await conn.ExecuteAsync(@"INSERT INTO arrivalgoods (ArrivalId, GoodId, Count, Price, PriceSell, Nds, ExpiresDate) VALUES (@ArrivalId, @GoodId, @Count, @Price, @PriceSell, @Nds, @ExpiresDate)", new
                {
                    ArrivalId = id,
                    GoodId = arrivalGood.GoodId,
                    Count = arrivalGood.Count,
                    Price = arrivalGood.Price,
                    PriceSell = arrivalGood.PriceSell,
                    Nds = arrivalGood.Nds,
                    ExpiresDate = arrivalGood.ExpiresDate
                });
            var balanceList = entity.ArrivalGoods.Select(a => new GoodCountBalanceCurrentLegacy
            {
                GoodId = a.GoodId,
                Count = a.Count
            });
            await CurrentBalanceChange(transaction, balanceList);
            var goodprices = entity.ArrivalGoods.GroupBy(b => b.GoodId).Select(x => new { GoodId = x.Key, Price = x.First().PriceSell }).ToDictionary(x => x.GoodId, x => x.Price);
            await PriceChange(transaction, goodprices);
            return id;
        }

        public async Task DeleteAsync(int id)
        {
            using(var con = new MySqlConnection(_connectionString))
            {
                con.Open();
                var transaction = con.BeginTransaction();
                try
                {
                    var arrivalGoods = await con.QueryAsync<ArrivalGoodLegacy>("SELECT * FROM arrivalgoods WHERE ArrivalId=" + id);
                    var balanceList = arrivalGoods.Select(a => new GoodCountBalanceCurrentLegacy
                    {
                        GoodId = a.GoodId,
                        Count = -1 * a.Count
                    });
                    await CurrentBalanceChange(transaction, balanceList);
                    await con.ExecuteAsync("DELETE FROM Arrivals WHERE id=" + id);
                }
                catch(Exception ex)
                {
                    transaction.Rollback();
                    throw ex;
                }
            }
        }

        public Task<IReadOnlyList<ArrivalLegacy>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<ArrivalLegacy> GetByIdAsync(int id)
        {
            using (var con = new MySqlConnection(_connectionString))
                return await getByIdAsync(con, id);
        }

        public void SetConnectionString(string connectionString) => _connectionString = connectionString;

        public async Task UpdateAsync(ArrivalLegacy entity)
        {
            using (var con = new MySqlConnection(_connectionString))
            {
                con.Open();
                var transaction = con.BeginTransaction();
                try
                {
                    var arrival = await getByIdAsync(con, entity.Id);
                    var balanceList = arrival.ArrivalGoods.Select(a => new GoodCountBalanceCurrentLegacy
                    {
                        GoodId = a.GoodId,
                        Count = -1 * a.Count
                    });
                    await CurrentBalanceChange(transaction, balanceList);

                    await con.ExecuteAsync("DELETE FROM arrivalgoods WHERE ArrivalId=" + entity.Id);

                    await con.ExecuteAsync(@"UPDATE Arrivals SET Num=@Num, DateArrival=@DateArrival, SupplierId=@SupplierId, 
SumPayments=@SumPayments, SumArrival=@SumArrival, SumNds=@SumNds, SumSell=@SumSell WHERE id=@Id", new
                    {
                        Id = entity.Id,
                        Num = entity.Num,
                        DateArrival = entity.DateArrival,
                        SupplierId = entity.SupplierId,
                        SumPayments = entity.SumPayments,
                        SumArrival = entity.SumArrival,
                        SumNds = entity.SumNds,
                        SumSell = entity.SumSell
                    });
                    foreach(var arrivalGood in entity.ArrivalGoods)
                        await con.ExecuteAsync(@"INSERT INTO arrivalgoods (ArrivalId, GoodId, Count, Price, PriceSell, Nds, ExpiresDate) VALUES (@ArrivalId, @GoodId, @Count, @Price, @PriceSell, @Nds, @ExpiresDate)", new
                        {
                            ArrivalId = entity.Id,
                            GoodId = arrivalGood.GoodId,
                            Count = arrivalGood.Count,
                            Price = arrivalGood.Price,
                            PriceSell = arrivalGood.PriceSell,
                            Nds = arrivalGood.Nds,
                            ExpiresDate = arrivalGood.ExpiresDate
                        });
                    balanceList = arrival.ArrivalGoods.Select(a => new GoodCountBalanceCurrentLegacy
                    {
                        GoodId = a.GoodId,
                        Count = a.Count
                    });
                    await CurrentBalanceChange(transaction, balanceList);

                    transaction.Commit();
                }
                catch(Exception ex)
                {
                    transaction.Rollback();
                    throw ex;
                }

            }
        }

        private async Task<ArrivalLegacy> getByIdAsync(MySqlConnection con, int id)
        {
            var arrival = await con.QueryFirstOrDefaultAsync<ArrivalLegacy>("SELECT * FROM arrivals WHERE id=" + id);
            arrival.ArrivalGoods = (await con.QueryAsync<ArrivalGoodLegacy>("SELECT * FROM arrivalgoods WHERE ArrivalId=" + id)).ToList();
            return arrival;
        }

        private async Task CurrentBalanceChange(MySqlTransaction transaction, IEnumerable<GoodCountBalanceCurrentLegacy> balances)
        {
            var conn = transaction.Connection;
            foreach (var balance in balances)
                await conn.ExecuteAsync("UPDATE goodcountbalancecurrents SET Count=Count+@Count WHERE GoodId=@GoodId", new
                {
                    GoodId=balance.GoodId,
                    Count = balance.Count
                });
        }

        private async Task PriceChange(MySqlTransaction transaction, IDictionary<int, decimal> prices)
        {
            foreach (var keyvalue in prices)
                await transaction.Connection.ExecuteAsync("UPDATE goodprices SET Price=@Price WHERE GoodId=@GoodId", new
                {
                    GoodId=keyvalue.Key,
                    Price= keyvalue.Value
                });

        }
    }
}
