using Dapper;
using MySql.Data.MySqlClient;
using OnlineShop2.LegacyDb.Infrastructure;
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
    public interface IGoodReporitoryLegacy : IGeneralRepositoryLegacy<GoodLegacy>
    {
        Task<int> AddAsync(GoodLegacy entity, int shopLegacyId);
        Task<IReadOnlyCollection<GoodLegacy>> AddRangeAsync(IEnumerable<GoodLegacy> entities, int shopLegacyId);
        Task<IEnumerable<GoodLegacy>> GetNewGoods();
        Task<IEnumerable<GoodLegacy>> GetUpdateGoods();
        Task SetProccessComplite();
    }
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
                bool flagDelete = false;
                int count = await con.QuerySingleAsync<int>("SELECT COUNT(*) FROM arrivalgoods WHERE GoodId=" + id);
                flagDelete = count > 0 ? flagDelete : true;
                if (!flagDelete)
                {
                    count = await con.QuerySingleAsync<int>("SELECT COUNT(*) FROM checkgoods WHERE GoodId=" + id);
                    flagDelete = count > 0 ? flagDelete : true;
                }
                if (!flagDelete)
                {
                    count = await con.QuerySingleAsync<int>("SELECT COUNT(*) FROM stocktakinggoods WHERE GoodId=" + id);
                    flagDelete = count > 0 ? flagDelete : true;
                }
                if (!flagDelete)
                {
                    count = await con.QuerySingleAsync<int>("SELECT COUNT(*) FROM writeofgoods WHERE GoodId=" + id);
                    flagDelete = count > 0 ? flagDelete : true;
                }
                if (!flagDelete)
                {
                    await con.ExecuteAsync("UPDATE Goods SET IsDeleted=1 WHERE id=" + id);
                    await con.ExecuteAsync("DELETE FROM barcodes WHERE GoodId=" + id);
                }
                else
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
                    var goodOriginal = await con.QuerySingleAsync<GoodLegacy>("SELECT * FROM goods WHERE id=" + good.Id);
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
                    if (goodOriginal.Price != good.Price)
                    {
                        //TODO: Добавить оформление переоценки в случае изменения цены
                    }

                    var shopLegacyId = await con.QuerySingleAsync<int>("SELECT id FROM shops");
                    await con.ExecuteAsync("DELETE FROM goodprices WHERE GoodId=" + good.Id);
                    var price = new GoodPriceLegacy { Price= good.Price, GoodId=good.Id };
                    good.GoodPrices.Add(price);
                    price.Id = await con.QuerySingleAsync<int>(@"INSERT INTO goodprices (GoodId, ShopId, Price) VALUES (@GoodId, @ShopId, @Price); SELECT LAST_INSERT_ID()", new
                    {
                        GoodId = good.Id,
                        ShopId = shopLegacyId,
                        Price = price.Price
                    });
                    await con.ExecuteAsync("DELETE FROM barcodes WHERE GoodId=" + good.Id);
                    if (!good.IsDeleted)
                        foreach (var barcode in good.Barcodes)
                        {
                            var barcodeInOtherRow = await con.QueryFirstOrDefaultAsync<string>("SELECT Code FROM barcodes WHERE Code=@Code AND GoodId<>@GoodId", new
                            {
                                GoodId=good.Id,
                                Code = barcode.Code
                            });
                            if(barcodeInOtherRow!=null)
                                throw new MyServiceLegacyException($"Штрих код {barcodeInOtherRow} уже существует");
;                            barcode.Id = await con.QuerySingleAsync<int>("INSERT INTO barcodes (GoodId, Code) VALUES (@GoodId, @Code); SELECT LAST_INSERT_ID()", new
                            {
                                GoodId = good.Id,
                                Code = barcode.Code
                            });
                        }
                    await tran.CommitAsync();
                }
                catch (MySqlException ex)
                {
                    tran.Rollback();
                    throw ex;
                }

            }
        }

        private List<int> processedDocumentsIds = new();
        private class DocumentHistoryModel
        {
            public int Id { get; set; }
            public int DocumentId { get; set; }
        }
        public async Task<IEnumerable<GoodLegacy>> GetNewGoods()
        {
            using var con = new MySqlConnection(_connectionString);
            con.Open();
            var documentsIds = await con.QueryAsync<DocumentHistoryModel>("SELECT id, DocumentId FROM documenthistories WHERE DocumentType=3 AND Processed=0");
            return await getNewOrUpdateGoods(con, documentsIds);
        }

        public async Task<IEnumerable<GoodLegacy>> GetUpdateGoods()
        {
            using var con = new MySqlConnection(_connectionString);
            con.Open();
            var documentsIds = await con.QueryAsync<DocumentHistoryModel>("SELECT id, DocumentId FROM documenthistories WHERE DocumentType=4 AND Processed=0");
            return await getNewOrUpdateGoods(con, documentsIds);
        }

        private async Task<IEnumerable<GoodLegacy>> getNewOrUpdateGoods(MySqlConnection con, IEnumerable<DocumentHistoryModel> documentsIds)
        {
            if (documentsIds.Count() == 0)
                return new List<GoodLegacy>();
            processedDocumentsIds.AddRange(documentsIds.Select(x => x.Id));
            var goodsIds = documentsIds.Select(x => x.DocumentId);
            var result = await con.QueryAsync<GoodLegacy>("SELECT * FROM Goods WHERE id IN @GoodsIds",
                new { GoodsIds = goodsIds });

            var barcodes = await con.QueryAsync<BarCodeLegacy>("SELECT * FROM Barcodes WHERE GoodId IN @GoodsIds",
                new { GoodsIds = goodsIds });
            foreach (var barcode in barcodes)
                result.First(x => x.Id == barcode.GoodId).Barcodes.Add(barcode);

            var prices = await con.QueryAsync<GoodPriceLegacy>("SELECT * FROM goodprices WHERE GoodId IN @GoodsIds",
                new { GoodsIds = goodsIds });
            foreach (var price in prices)
                result.First(x => x.Id == price.GoodId).GoodPrices.Add(price);

            return result;
        }

        public async Task SetProccessComplite()
        {
            if (processedDocumentsIds.Count() == 0)
                return;
            using var con = new MySqlConnection(_connectionString);
            con.Open();
            await con.ExecuteAsync("UPDATE documenthistories SET Processed=1 WHERE id IN @Ids", 
                new {Ids = processedDocumentsIds });
        }
    }
}
