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
    public class GoodGroupRepositoryLegacy:IGoodGroupRepositoryLegacy
    {
        private string _connectionString;

        public void SetConnectionString(string connectionString) => _connectionString = connectionString;

        public async Task<int> AddAsync(GoodGroupLegacy entity)
        {
            using(MySqlConnection con = new MySqlConnection(_connectionString))
            {
                con.Open();
                return await con.QuerySingleAsync<int>("INSERT INTO goodgroups (Name) VALUES (@Name); SELECT LAST_INSERT_ID()", new
                {
                    Name= entity.Name
                });
            }
        }

        public async Task<IReadOnlyCollection<GoodGroupLegacy>> AddRangeAsync(IEnumerable<GoodGroupLegacy> entities)
        {
            using (MySqlConnection con = new MySqlConnection(_connectionString))
            {
                con.Open();
                var tran = con.BeginTransaction();
                try
                {
                    foreach (var entity in entities)
                        entity.Id = await con.QuerySingleAsync<int>("INSERT INTO goodgroups (Name) VALUES (@Name); SELECT LAST_INSERT_ID()", new
                        {
                            Name = entity.Name
                        });
                    tran.Commit();
                }
                catch(MySqlException ex)
                {
                    foreach (var entity in entities)
                        entity.Id = 0;
                        tran.Rollback();
                    throw ex;
                }
            }
            return entities.ToList();
        }

        public async Task DeleteAsync(int id)
        {
            using (MySqlConnection con = new MySqlConnection(_connectionString))
            {
                con.Open();
                await con.ExecuteAsync("DELETE FROM goodgroups WHERE id=" + id);
            }
        }

        public async Task<IReadOnlyList<GoodGroupLegacy>> GetAllAsync()
        {
            using (MySqlConnection con = new MySqlConnection(_connectionString))
            {
                con.Open();
                return (await con.QueryAsync<GoodGroupLegacy>("SELECT * FROM goodgroups")).ToList();
            }
        }

        public async Task<GoodGroupLegacy> GetByIdAsync(int id)
        {
            using (MySqlConnection con = new MySqlConnection(_connectionString))
            {
                con.Open();
                return await con.QuerySingleAsync<GoodGroupLegacy>("SELECT * FROM goodgroups WHERE id=" + id);
            }
        }

        public async Task UpdateAsync(GoodGroupLegacy entity)
        {
            using (MySqlConnection con = new MySqlConnection(_connectionString))
            {
                con.Open();
                await con.ExecuteAsync("UPDATE goodgroups SET Name=@Name WHERE id=@Id",new
                {
                    Id=entity.Id, Name= entity.Name
                });
            }
        }
    }
}
