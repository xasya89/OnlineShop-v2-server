using Dapper;
using MySql.Data.MySqlClient;
using OnlineShop2.LegacyDb.Infrastructure;
using OnlineShop2.LegacyDb.Models;
using Org.BouncyCastle.Math.EC.Multiplier;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Dapper.SqlMapper;

namespace OnlineShop2.LegacyDb.Repositories
{
    public interface IWriteofRepositoryLegacy : IGeneralRepositoryLegacy<WriteofLegacy>
    {
        Task<IEnumerable<WriteofLegacy>> GetWriteOfWithDate(DateTime with);
    }
    public class WriteofRepositoryLegacy : IWriteofRepositoryLegacy
    {
        private string _connectionString;

        public async Task<int> AddAsync(WriteofLegacy entity)
        {
            using MySqlConnection con = new MySqlConnection(_connectionString);
            con.Open();
            var transaction = con.BeginTransaction();
            try
            {
                int writeofId = await con.QuerySingleAsync<int>(@"INSERT INTO writeofs (DateWriteof, ShopId, Note, SumAll, IsSuccess, Status, Uuid) 
VALUES (@DateWriteof, 1, @Note, @SumAll, 1, @Status, @Uuid); SELECT LAST_INSERT_ID()", 
                new { DateWriteof = entity.DateWriteof, Note=entity.Note, SumAll=entity.SumAll, Status = entity.Status, Uuid=entity.Uuid});
                foreach (var position in entity.WriteofGoods)
                    await con.ExecuteAsync(@"INSERT INTO writeofgoods (WriteofId, GoodId, Price, Count) 
VALUES (@WriteofId, @GoodId, @Price, @Count)",
                new { WriteofId = writeofId, GoodId = position.GoodId, Price = position.Price, Count = position.Count });

                var balanceMinus = entity.WriteofGoods.GroupBy(w => w.GoodId).ToDictionary(x => x.Key, x => -1 * x.Sum(x => x.Count));
                await CurrentBalanceChange.ChangeBalance(con, balanceMinus);

                await transaction.CommitAsync();
                return writeofId;
            }
            catch(MySqlException ex)
            {
                await transaction.RollbackAsync();
                throw ex;
            }
        }

        public Task<IReadOnlyCollection<WriteofLegacy>> AddRangeAsync(IEnumerable<WriteofLegacy> entities)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteAsync(int id)
        {
            using MySqlConnection con = new MySqlConnection(_connectionString);
            con.Open();
            var transaction = con.BeginTransaction();
            try
            {
                var positions = await con.QueryAsync<WriteofGoodLegacy>("SELECT * FROM writeofgoods WHERE WriteofId=@WriteofId", new { WriteofId = id });

                var balancePlus = positions.GroupBy(w => w.GoodId).ToDictionary(x => x.Key, x => x.Sum(x => x.Count));
                await CurrentBalanceChange.ChangeBalance(con, balancePlus);

                await con.ExecuteAsync("DELETE FROM writeofs WHERE id = @Id", new { Id = id });

                await transaction.CommitAsync();
            }
            catch(MySqlException ex)
            {
                await transaction.RollbackAsync();
                throw ex;
            }
        }

        public Task<IReadOnlyList<WriteofLegacy>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<WriteofLegacy>> GetWriteOfWithDate(DateTime with)
        {
            var withDateWithoutTime = DateOnly.FromDateTime(with).ToDateTime(TimeOnly.MinValue);
            using MySqlConnection con = new MySqlConnection(_connectionString);
            con.Open();
            var writeofs = await con.QueryAsync<WriteofLegacy>("SELECT * FROM writeofs WHERE DateWriteof>=@DateWriteof", new { DateWriteof = withDateWithoutTime });
            var positions = await con.QueryAsync<WriteofGoodLegacy>("SELECT * FROM writeofgoods WHERE WriteofId IN @Ids",
                new { Ids = writeofs.Select(w => w.Id) });
            foreach(var writeof in writeofs)
                writeof.WriteofGoods = positions.Where(p=>p.WriteofId==writeof.Id).ToList();
            return writeofs;
        }

        public async Task<WriteofLegacy> GetByIdAsync(int id)
        {
            using MySqlConnection con = new MySqlConnection(_connectionString);
            con.Open();
            var writeof = await con.QuerySingleAsync<WriteofLegacy>("SELECT * FROM writeofs WHERE id>=@Id", new { Id = id });
            writeof.WriteofGoods = (await getPositions(con, id)).ToList();
            return writeof;
        }

        public void SetConnectionString(string connectionString) => _connectionString = connectionString;

        public async Task UpdateAsync(WriteofLegacy entity)
        {
            using MySqlConnection con = new MySqlConnection(_connectionString);
            con.Open();
            var transaction = con.BeginTransaction();
            try
            {
                var originalGoods = (await getPositions(con, entity.Id)).GroupBy(x=>x.GoodId).ToDictionary(x=>x.Key, x=>x.Sum(x=>x.Count));
                await CurrentBalanceChange.ChangeBalance(con, originalGoods);

                await con.ExecuteAsync("DELETE FROM writeofgoods WHERE WriteofId=@WriteofId", new { WriteofId = entity.Id });
                await con.ExecuteAsync("UPDATE writeofs SET DateWriteof=@DateWriteof, Note=@Note, SumAll=@SumAll WHERE Id=@Id",
                    new { DateWriteof=entity.DateWriteof, Note=entity.Note, SumAll=entity.SumAll, Id=entity.Id });
                foreach (var position in entity.WriteofGoods)
                    await con.ExecuteAsync(@"INSERT INTO writeofgoods (WriteofId, GoodId, Price, Count) 
VALUES (@WriteofId, @GoodId, @Price, @Count)",
                new { WriteofId = entity.Id, GoodId = position.GoodId, Price = position.Price, Count = position.Count });

                await CurrentBalanceChange.ChangeBalance(con, entity.WriteofGoods.GroupBy(x => x.GoodId).ToDictionary(x => x.Key, x => -1 * x.Sum(x => x.Count)));

                await transaction.CommitAsync();
            }
            catch(MySqlException ex)
            {
                await transaction.RollbackAsync();
                throw ex;
            }
        }

        private async Task<IEnumerable<WriteofGoodLegacy>> getPositions(MySqlConnection con, int writeofId) =>
            await con.QueryAsync<WriteofGoodLegacy>("SELECT * FROM writeofgoods WHERE WriteofId=@Id",
                new { Id = writeofId });
    }
}
