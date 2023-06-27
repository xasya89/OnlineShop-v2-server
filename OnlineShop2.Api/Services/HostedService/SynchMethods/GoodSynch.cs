using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using OnlineShop2.Database;
using OnlineShop2.Database.Models;
using OnlineShop2.LegacyDb.Models;
using OnlineShop2.LegacyDb.Repositories;
using System.Text.RegularExpressions;

namespace OnlineShop2.Api.Services.HostedService.SynchMethods
{
    public static class GoodSynch
    {
        public static async Task StartSynch(OnlineShopContext context, 
            GoodService goodService, 
            IGoodReporitoryLegacy reporitoryLegacy, 
            IMapper mapper,
            int shopId, 
            bool ownerGoodForShops=true)
        {
            var shops = await context.Shops.AsNoTracking().ToListAsync();
            var transaction = context.Database.BeginTransaction();
            try
            {
                if(await compareCountGoods(context, reporitoryLegacy))
                    await synchAll(context, reporitoryLegacy, shopId);
                else
                {
                    await synchNewGood(context, goodService, reporitoryLegacy, mapper, shopId, shops);
                    await synchUpdateGoods(context, goodService, reporitoryLegacy, mapper, shopId);
                }

                await context.SaveChangesAsync();
                await reporitoryLegacy.SetProccessComplite();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new Exception(nameof(GoodSynch), ex);
            }
            
        }

        private static async Task<bool> compareCountGoods(OnlineShopContext context, IGoodReporitoryLegacy repositoryLegacy)
        {
            var dbCount = await context.Goods.CountAsync();
            var legacyCount = (await repositoryLegacy.GetAllAsync()).Count;
            return dbCount < legacyCount;
        }

        private static async Task synchAll(OnlineShopContext context, IGoodReporitoryLegacy reporitoryLegacy, int shopId)
        {
            var goodsLegacy = await reporitoryLegacy.GetAllAsync();
            var goods = await context.Goods.Include(x => x.GoodPrices).Include(x => x.Barcodes).AsNoTracking().ToListAsync();

            var groups = await context.GoodsGroups.AsNoTracking().ToListAsync();
            var suppliers = await context.Suppliers.AsNoTracking().ToListAsync();

            var newGoods = from goodLegacy in goodsLegacy
                           join good in goods on goodLegacy.Id equals good.LegacyId into t
                           from subGood in t.DefaultIfEmpty()
                           where subGood == null
                           select new Good
                           {
                               ShopId = shopId,
                               Name = goodLegacy.Name,
                               Article = goodLegacy.Article,
                               GoodGroupId = groups.Where(x => x.LegacyId == goodLegacy.GoodGroupId).First().Id,
                               SupplierId = suppliers.Where(x => x.LegacyId == goodLegacy.SupplierId).FirstOrDefault()?.Id,
                               Unit = goodLegacy.Unit,
                               Price = goodLegacy.Price,
                               GoodPrices = goodLegacy.GoodPrices.Select(p => new GoodPrice { Price = p.Price, ShopId = shopId }).ToList(),
                               Barcodes = goodLegacy.Barcodes.Select(b => new Barcode { Code = b.Code ?? "1" }).ToList(),
                               SpecialType = goodLegacy.SpecialType,
                               VPackage = goodLegacy.VPackage,
                               IsDeleted = goodLegacy.IsDeleted,
                               Uuid = goodLegacy.Uuid,
                               LegacyId = goodLegacy.Id,
                               CurrentBalances = context.Shops.Select(s => new GoodCurrentBalance { ShopId = s.Id }).ToList()
                           };
            context.Goods.AddRange(newGoods);
        }

        private static async Task synchNewGood(OnlineShopContext context,
            GoodService goodService,
            IGoodReporitoryLegacy reporitoryLegacy,
            IMapper mapper,
            int shopId,
            IEnumerable<Shop> shops,
            bool ownerGoodForShops = true)
        {
            var newGoods = mapper.Map<IEnumerable<Good>>(await reporitoryLegacy.GetNewGoods());

            var groupsLegacyIds = newGoods.Select(x => x.GoodGroupId);
            var supplersLegacyIds = newGoods.Select(x => x.SupplierId);
            var groups = await context.GoodsGroups.Where(x => groupsLegacyIds.Contains(x.LegacyId ?? 0)).AsNoTracking().ToListAsync();
            var suppliers = await context.Suppliers.Where(x=>supplersLegacyIds.Contains(x.LegacyId ?? 0)).AsNoTracking().ToListAsync();

            foreach (var good in newGoods)
            {
                good.LegacyId = good.Id;
                good.Id = 0;
                good.ShopId = shopId;
                good.GoodGroupId = groups.First(x => x.LegacyId == good.GoodGroupId).Id;
                good.SupplierId = suppliers.FirstOrDefault(x => x.LegacyId == good.SupplierId)?.Id;
                foreach (var barcode in good.Barcodes)
                {
                    barcode.Id = 0;
                }
                if (ownerGoodForShops)
                    foreach (var price in good.GoodPrices)
                    {
                        price.Id = 0;
                        price.ShopId = shopId;
                    }
                else
                    good.GoodPrices = shops.Select(x => new GoodPrice
                    {
                        Price = good.GoodPrices.FirstOrDefault()?.Price ?? 0,
                        ShopId = shopId
                    }).ToList();
            };

            context.Goods.AddRange(newGoods);
        }

        private static async Task synchUpdateGoods(OnlineShopContext context,
            GoodService goodService,
            IGoodReporitoryLegacy reporitoryLegacy,
            IMapper mapper,
            int shopId)
        {
            var legacyGoods = mapper.Map<IEnumerable<Good>>(await reporitoryLegacy.GetUpdateGoods());
            IEnumerable<int> legacyIds = legacyGoods.Select(x => x.Id);
            var goods = await context.Goods.Include(x => x.GoodPrices).Include(x => x.Barcodes)
                .Where(x => legacyIds.Contains(x.LegacyId ?? 0)).AsNoTracking().ToListAsync();

            var groupsLegacyIds = legacyGoods.Select(x => x.GoodGroupId);
            var supplersLegacyIds = legacyGoods.Select(x => x.SupplierId);
            var groups = await context.GoodsGroups.Where(x => groupsLegacyIds.Contains(x.LegacyId ?? 0)).AsNoTracking().ToListAsync();
            var suppliers = await context.Suppliers.Where(x => supplersLegacyIds.Contains(x.LegacyId ?? 0)).AsNoTracking().ToListAsync();

            foreach(var good in legacyGoods)
            {
                var goodDb = goods.First(x => x.LegacyId == good.Id);
                good.LegacyId = good.Id;
                good.ShopId = goodDb.ShopId;
                good.Id = goodDb.Id;
                good.LegacyId = goodDb.LegacyId;
                foreach (var barcode in good.Barcodes)
                    barcode.Id = 0;
                foreach (var price in good.GoodPrices)
                {
                    price.Id = 0; 
                    price.ShopId = shopId;
                };
            }

            context.Goods.UpdateRange(legacyGoods);
        }
    }
}
