using Dapper;
using MySql.Data.MySqlClient;
using OnlineShop2.LegacyDb.Models;
using Org.BouncyCastle.Crypto.Tls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static Dapper.SqlMapper;

namespace OnlineShop2.LegacyDb.Repositories
{
    public class GoodRepositoryLegacy : IGoodReporitoryLegacy
    {
        private string _connectionString;
        public void SetConnectionString(string connectionString) => _connectionString = connectionString;
        public async Task<int> AddAsync(GoodLegacy entity) => await AddAsync(entity, 1);
        public async Task<int> AddAsync(GoodLegacy entity, int shopLegacyId)
        {
            using(MySqlConnection con=new MySqlConnection(_connectionString))
            {
                con.Open();
                var tran = con.BeginTransaction();
                try
                {
                    entity.Id = await con.QuerySingleAsync<int>(@"INSERT INTO goods (Name, Article, BarCode, Unit, Price, GoodGroupId, SupplierId, SpecialType, VPackage, IsDeleted, Uuid)
                    VALUES (@Name, @Article, @BarCode, @Unit, @Price, @GoodGroupId, @SupplierId, @SpecialType, @VPackage, @IsDeleted, @Uuid); SELECT LAST_INSERT_ID()", new
                    {
                        Name = entity.Name,
                        Article = entity.Article,
                        BarCode = entity.BarCode,
                        Unit = entity.Unit,
                        Price = entity.Price,
                        SpecialType = entity.SpecialType,
                        GoodGroupId = entity.GoodGroupId,
                        SupplierId = entity.SupplierId,
                        VPackage = entity.VPackage,
                        IsDeleted = entity.IsDeleted,
                        Uuid = Guid.NewGuid()
                    });
                    
                    foreach (var price in entity.GoodPrices.Where(p => p.Id == 0))
                    {
                        price.GoodId = entity.Id;
                        price.Id = await con.QuerySingleAsync<int>(@"INSERT INTO goodprices (GoodId, ShopId, Price) VALUES (@GoodId, @ShopId, @Price); SELECT LAST_INSERT_ID()", new
                        {
                            GoodId = entity.Id,
                            ShopId = shopLegacyId,
                            Price = price.Price
                        });
                    }
                    
                    foreach (var barcode in entity.Barcodes.Where(p => p.Id == 0))
                    {
                        barcode.GoodId = entity.Id;
                        barcode.Id = await con.QuerySingleAsync<int>("INSERT INTO barcodes (GoodId, Code) VALUES (@GoodId, @Code); SELECT LAST_INSERT_ID()", new
                        {
                            GoodId = entity.Id,
                            Code = barcode.Code
                        });
                    }
                    tran.Commit();
                }
                catch (MySqlException ex)
                {
                    tran.Rollback();
                    throw ex;
                }
            }
            return entity.Id;
        }

        public async Task<IReadOnlyCollection<GoodLegacy>> AddRangeAsync(IEnumerable<GoodLegacy> entities) =>
            await AddRangeAsync(entities, 1);
        public async Task<IReadOnlyCollection<GoodLegacy>> AddRangeAsync(IEnumerable<GoodLegacy> entities, int shopLegacyId)
        {
            foreach(var good in entities)
                good.Id= await AddAsync(good,shopLegacyId);
            return entities.ToList();
        }

        public async Task DeleteAsync(int id)
        {
            using (MySqlConnection con = new MySqlConnection(_connectionString))
            {
                con.Open();
                await con.ExecuteAsync("DELETE FROM Goods WHERE id=" + id);
            }
        }

        public async Task<IReadOnlyList<GoodLegacy>> GetAllAsync()
        {
            using (MySqlConnection con = new MySqlConnection(_connectionString))
            {
                con.Open();
                var goods = await con.QueryAsync<GoodLegacy>("SELECT * FROM Goods");
                var prices = await con.QueryAsync<GoodPriceLegacy>("SELECT * FROM goodprices");
                var barcodes = await con.QueryAsync<BarCodeLegacy>("SELECT * FROM barcodes");
                foreach (var good in goods)
                    good.GoodPrices = prices.Where(p => p.GoodId == good.Id).ToList();
                foreach (var good in goods)
                    good.Barcodes = barcodes.Where(p => p.GoodId == good.Id).ToList();
                return goods.ToList();
            }
        }

        public async Task<GoodLegacy> GetByIdAsync(int id)
        {
            using (MySqlConnection con = new MySqlConnection(_connectionString))
            {
                con.Open();
                var good = await con.QueryFirstOrDefaultAsync<GoodLegacy>("SELECT * FROM Goods WHERE id = " + id);
                var prices = await con.QueryAsync<GoodPriceLegacy>("SELECT * FROM goodprices");
                var barcodes = await con.QueryAsync<BarCodeLegacy>("SELECT * FROM barcodes");
                if (good != null)
                {
                    good.GoodPrices = prices.Where(p => p.GoodId == good.Id).ToList();
                    good.Barcodes = barcodes.Where(p => p.GoodId == good.Id).ToList();
                }
                return good;
            }
        }

        public async Task UpdateAsync(GoodLegacy good)
        {
            using (MySqlConnection con = new MySqlConnection(_connectionString))
            {
                con.Open();
                var tran = con.BeginTransaction();
                try
                {
                    await con.ExecuteAsync(@"UPDATE Goods SET Name=@Name, Article=@Article, BarCode=@BarCode, Unit=@Unit, Price=@Price,
                    Uuid=@Uuid, GoodGroupId=@GoodGroupId, SupplierId=@SupplierId, SpecialType=@SpecialType, VPackage=@VPackage, IsDeleted=@IsDeleted
                    WHERE id=@Id", new
                    {
                        Name = good.Name,
                        Article = good.Article,
                        BarCode = good.BarCode,
                        Unit = good.Unit,
                        Price = good.Price,
                        Uuid = good.Uuid,
                        GoodGroupId = good.GoodGroupId,
                        SupplierId = good.SupplierId,
                        SpecialType = good.SpecialType,
                        VPackage = good.VPackage,
                        IsDeleted = good.IsDeleted,
                        Id = good.Id
                    });
                    var shopLegacyId = await con.QuerySingleAsync<int>("SELECT id FROM shops");
                    foreach (var price in good.GoodPrices)
                        if (price.Id != 0)
                            await con.ExecuteAsync("UPDATE goodprices SET Price=@Price WHERE id=@Id", new { Id = price.Id, Price = good.Price });
                        else
                            price.Id = await con.QuerySingleAsync<int>(@"INSERT INTO goodprices (GoodId, ShopId, Price) VALUES (@GoodId, @ShopId, @Price); SELECT LAST_INSERT_ID()", new
                            {
                                GoodId = good.Id,
                                ShopId = shopLegacyId,
                                Price = price.Price
                            });
                    foreach (var barcode in good.Barcodes)
                        if (barcode.Id != 0)
                            await con.ExecuteAsync("UPDATE barcodes SET Code=@Code WHERE id=@Id", new { Id = barcode.Id, Code = barcode.Code });
                        else
                            barcode.Id = await con.QuerySingleAsync<int>("INSERT INTO barcodes (GoodId, Code) VALUES (@GoodId, @Code); SELECT LAST_INSERT_ID()", new
                            {
                                GoodId = good.Id,
                                Code = barcode.Code
                            });
                }
                catch (MySqlException ex)
                {
                    tran.Rollback();
                    throw ex;
                }

            }
        }
    }
}
