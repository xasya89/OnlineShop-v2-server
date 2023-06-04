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
    public class SupplierRepositoryLegacy : ISupplierRepositoryLegacy
    {
        public string _connStr;

        public void SetConnectionString(string connectionString) => _connStr = connectionString;

        public async Task<int> AddAsync(SupplierLegacy entity)
        {
            using(var con = new MySqlConnection(_connStr))
            {
                con.Open();
                return await con.QuerySingleAsync<int>("INSERT INTO suppliers (Name, Inn) VALUES (@Name, @Inn); SELECT LAST_INSERT_ID()", new
                {
                    Name = entity.Name, Inn=entity.Inn
                });
            }
        }

        public async Task<IReadOnlyCollection<SupplierLegacy>> AddRangeAsync(IEnumerable<SupplierLegacy> entities)
        {
            using (var con = new MySqlConnection(_connStr))
            {
                con.Open();
                var tran = con.BeginTransaction();
                try
                {
                    foreach (var entity in entities)
                        entity.Id = await con.QuerySingleAsync("INSERT INTO suppliers (Name, Inn) VALUES (@Name, @Inn); SELECT LAST_INSERT_ID()", new
                        {
                            Name = entity.Name,
                            Inn = entity.Inn
                        });
                    tran.Commit();
                }
                catch (MySqlException ex)
                {
                    tran.Rollback();
                    foreach (var entity in entities)
                        entity.Id = 0;
                    throw ex;
                }

            }
            return entities.ToList();
        }

        public async Task DeleteAsync(int id)
        {
            using (var con = new MySqlConnection(_connStr))
            {
                con.Open();
                await con.ExecuteAsync("UPDATE Goods SET SupplierId=null WHERE SupplierId=" + id);
                await con.ExecuteAsync("DELETE FROM suppliers WHERE id = " + id);
            }
        }

        public async Task<IReadOnlyList<SupplierLegacy>> GetAllAsync()
        {
            using (var con = new MySqlConnection(_connStr))
            {
                con.Open();
                return (await con.QueryAsync<SupplierLegacy>("SELECT * FROM suppliers")).ToList();
            }
        }

        public async Task<SupplierLegacy> GetByIdAsync(int id)
        {
            using (var con = new MySqlConnection(_connStr))
            {
                con.Open();
                return await con.QueryFirstAsync<SupplierLegacy>("SELECT * FROM suppliers WHERE id = "+id);
            }
        }

        public async Task UpdateAsync(SupplierLegacy entity)
        {
            using (var con = new MySqlConnection(_connStr))
            {
                con.Open();
                await con.ExecuteAsync("UPDATE suppliers SET Name=@Name, Inn=@Inn WHERE id=@Id ", new {
                    Name = entity.Name, Inn=entity.Inn, Id=entity.Id
                });
            }
        }
    }
}
