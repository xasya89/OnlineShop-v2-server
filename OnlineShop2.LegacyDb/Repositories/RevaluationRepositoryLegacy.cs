using OnlineShop2.LegacyDb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using MySql.Data.MySqlClient;
using static Dapper.SqlMapper;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using Microsoft.VisualBasic;
using Org.BouncyCastle.Operators;

namespace OnlineShop2.LegacyDb.Repositories
{
    public interface IRevaluationRepositoryLegacy:IGeneralRepositoryLegacy<RevaluationLegacy>
    {
        Task<IEnumerable<RevaluationLegacy>> GetWithDate(DateTime with);
    }
    public class RevaluationRepositoryLegacy : IRevaluationRepositoryLegacy
    {
        private string _connectionString;

        public void SetConnectionString(string connectionString) => _connectionString = connectionString;


        public async Task<int> AddAsync(RevaluationLegacy entity)
        {
            using MySqlConnection con = new MySqlConnection(_connectionString);
            con.Open();
            var tran = await con.BeginTransactionAsync();
            try
            {
                int id = await con.QuerySingleAsync<int>(@"INSERT INTO revaluations (Create, Status, SumNew, SumOld, Uuid) 
                    VALUES (@Create, 2, @SumNew, @SumOld, @Uuid); SELECT LAST_INSERT_ID()",
                    new {
                        Create = DateOnly.FromDateTime(entity.Create).ToDateTime(TimeOnly.MinValue),
                        SumNew = entity.RevaluationGoods.Sum(x=>x.PriceNew * x.Count),
                        SumOld = entity.RevaluationGoods.Sum(x => x.PriceOld * x.Count),
                        Uuid = Guid.NewGuid()
                    });
                foreach (var item in entity.RevaluationGoods)
                    await con.ExecuteAsync("INSERT INTO revaluationgoods (RevaluationId, GoodId, Count, PriceOld, PriceNew)" +
                        "VALUES (@RevaluationId, @GoodId, @Count, @PriceOld, @PriceNew)",
                        new { RevaluationId = id, GoodId = item.GoodId, Count = item.Count, PriceOld = item.PriceOld, PriceNew = item.PriceNew });
                await tran.CommitAsync();
                return id;
            }
            catch(Exception ex)
            {
                await tran.RollbackAsync();
                throw ex;
            }
            
        }

        public async Task<IReadOnlyCollection<RevaluationLegacy>> AddRangeAsync(IEnumerable<RevaluationLegacy> entities)
        {
            using MySqlConnection con = new MySqlConnection(_connectionString);
            con.Open();
            var tran = await con.BeginTransactionAsync();
            try
            {
                foreach(var  entity in entities)
                {
                    entity.Id = await con.QuerySingleAsync<int>(@"INSERT INTO revaluations (Create, Status, SumNew, SumOld, Uuid) 
                    VALUES (@Create, 2, @SumNew, @SumOld, @Uuid); SELECT LAST_INSERT_ID()",
                    new
                    {
                        Create = DateOnly.FromDateTime(entity.Create).ToDateTime(TimeOnly.MinValue),
                        SumNew = entity.RevaluationGoods.Sum(x => x.PriceNew * x.Count),
                        SumOld = entity.RevaluationGoods.Sum(x => x.PriceOld * x.Count),
                        Uuid = Guid.NewGuid()
                    });
                    foreach (var item in entity.RevaluationGoods)
                        item.Id = await con.ExecuteAsync("INSERT INTO revaluationgoods (RevaluationId, GoodId, Count, PriceOld, PriceNew)" +
                            "VALUES (@RevaluationId, @GoodId, @Count, @PriceOld, @PriceNew); SELECT LAST_INSERT_ID()",
                            new { RevaluationId = entity.Id, GoodId = item.GoodId, Count = item.Count, PriceOld = item.PriceOld, PriceNew = item.PriceNew });
                }
                await tran.CommitAsync();
                return entities.ToImmutableList();
            }
            catch (Exception ex)
            {
                await tran.RollbackAsync();
                throw ex;
            }
        }

        public async Task DeleteAsync(int id)
        {
            using MySqlConnection con = new MySqlConnection(_connectionString);
            con.Open();
            await con.ExecuteAsync("DELETE FROM revaluations WHERE id = " + id);
        }

        public async Task<IReadOnlyList<RevaluationLegacy>> GetAllAsync()
        {
            using MySqlConnection con = new MySqlConnection(_connectionString);
            con.Open();
            var revaluations = await con.QueryAsync<RevaluationLegacy>("SELECT * FROM revaluations ORDER BY Create DESC LIMIT 100");
            var ids = revaluations.Select(x => x.Id);
            var revaluationsGoods = await con.QueryAsync<RevaluationGoodLegacy>("SELECT * FROM revaluationgoods WHERE id IN @Ids",
                new { Ids = ids });
            foreach (var revaluation in revaluations)
                revaluation.RevaluationGoods = revaluationsGoods.Where(x => x.RevaluationId == revaluation.Id).ToList();

            return revaluations.ToImmutableList();
        }

        public async Task<RevaluationLegacy> GetByIdAsync(int id)
        {
            using MySqlConnection con = new MySqlConnection(_connectionString);
            con.Open();
            var revaluation = await con.QueryFirstAsync<RevaluationLegacy>("SELECT * FROM revaluations WHERE id=" + id);
            var revaluationGoods = await con.QueryAsync<RevaluationGoodLegacy>("SELECT * FROM revaluationgoods WHERE RevaluationId = " + id);
            revaluation.RevaluationGoods = revaluationGoods.ToList();
            return revaluation;
        }

        public async Task<IEnumerable<RevaluationLegacy>> GetWithDate(DateTime with)
        {
            var dateWithoutTime = DateOnly.FromDateTime(with).ToDateTime(TimeOnly.MinValue);
            using MySqlConnection con = new MySqlConnection(_connectionString);
            con.Open();
            var revaluations = await con.QueryAsync<RevaluationLegacy>("SELECT * FROM revaluations WHERE Create>=@Create",
                new {Create = dateWithoutTime});
            var ids = revaluations.Select(x => x.Id);
            var revaluationsGoods = await con.QueryAsync<RevaluationGoodLegacy>("SELECT * FROM revaluationgoods WHERE RevaluationId IN @Ids",
                new { Ids = ids });
            foreach (var revaluation in revaluations)
                revaluation.RevaluationGoods = revaluationsGoods.Where(x => x.RevaluationId == revaluation.Id).ToList();
            return revaluations;
        }

        public async Task UpdateAsync(RevaluationLegacy entity)
        {
            using MySqlConnection con = new MySqlConnection(_connectionString);
            con.Open();
            var tran = await con.BeginTransactionAsync();
            try
            {
                await con.ExecuteAsync("UPDATE revaluations SET Create=@Create, SumNew=@SumNew, SumOld=@SumOld WHERE id=@Id",
                    new {
                        Id = entity.Id,
                        Create = entity.Create,
                        SumNew = entity.RevaluationGoods.Sum(x=>x.PriceNew * x.Count),
                        SumOld = entity.RevaluationGoods.Sum(x => x.PriceOld * x.Count),
                    });
                await con.ExecuteAsync("DELETE FROM revaluationgoods WHERE RevaluationId = " + entity.Id);
                foreach (var item in entity.RevaluationGoods)
                    item.Id = await con.ExecuteAsync("INSERT INTO revaluationgoods (RevaluationId, GoodId, Count, PriceOld, PriceNew)" +
                        "VALUES (@RevaluationId, @GoodId, @Count, @PriceOld, @PriceNew); SELECT LAST_INSERT_ID()",
                        new { RevaluationId = entity.Id, GoodId = item.GoodId, Count = item.Count, PriceOld = item.PriceOld, PriceNew = item.PriceNew });
                await tran.CommitAsync();
            }
            catch(Exception ex)
            {
                await tran.RollbackAsync();
                throw ex;
            }
        }
    }
}
