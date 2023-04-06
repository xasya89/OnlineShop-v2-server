using Microsoft.EntityFrameworkCore;
using OnlineShop2.Database;
using OnlineShop2.Database.Models;
using OnlineShop2.LegacyDb;
using OnlineShop2.LegacyDb.Models;
using OnlineShop2.LegacyDb.Repositories;

namespace OnlineShop2.Api.Services.Legacy
{
    public class SynchLegacyService
    {
        private readonly OnlineShopContext _context;
        private readonly IConfiguration _configuration;
        public SynchLegacyService(OnlineShopContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task SynchGoods(int shopId, int shopNumLegacy)
        {
            string connstr = _configuration.GetConnectionString("shop" + shopNumLegacy);
            using (var unitOfWOrkLegacy = new UnitOfWorkLegacy(_configuration.GetConnectionString("shop" + shopNumLegacy)))
            {
                var repository = unitOfWOrkLegacy.GoodRepository;

                await synchSuppliers(repository, shopId);
                await synchGoodGroups(repository, shopId);
                await synchGoods(repository, shopId);
                await _context.SaveChangesAsync();
            }
            normalizeSequence();
        }

        private async Task synchSuppliers(GoodLegacyRepository repository, int shopId)
        {
            var suppliersLegacy = await repository.GetSuppliersAsync();

            var suppliers = await _context.Suppliers.AsNoTracking().ToListAsync();
            var newSuppliers = from supplierLegacy in suppliersLegacy
                               join supplier in suppliers on supplierLegacy.Id equals supplier.LegacyId into t
                               from subSupplier in t.DefaultIfEmpty()
                               where subSupplier == null
                               select new Supplier { Name = supplierLegacy.Name, ShopId = shopId, LegacyId = supplierLegacy.Id };
            _context.Suppliers.AddRange(newSuppliers);

            var changedSuppliers = from supplierLegacy in suppliersLegacy
                                   join supplier in suppliers on supplierLegacy.Id equals supplier.LegacyId into t
                                   from subSupplier in t.DefaultIfEmpty()
                                   where subSupplier != null && subSupplier.Name != supplierLegacy.Name
                                   select new { db = subSupplier, name = supplierLegacy.Name };
            foreach (var changeSupplier in changedSuppliers)
                {
                    _context.Entry(changeSupplier.db).State=EntityState.Modified;
                    changeSupplier.db.Name = changeSupplier.name;
                }

        }

        private async Task synchGoodGroups(GoodLegacyRepository repository, int shopId)
        {
            var goodGroupsLegacy = await repository.GetGroupsAsync();

            var goodGroups = await _context.GoodsGroups.AsNoTracking().ToListAsync();
            var newGroups = from goodGroupLegacy in goodGroupsLegacy
                            join goodGroup in goodGroups on goodGroupLegacy.Id equals goodGroup.LegacyId into t
                            from subGroup in t.DefaultIfEmpty()
                            where subGroup == null
                            select new GoodGroup { ShopId = shopId, Name = goodGroupLegacy.Name, LegacyId = goodGroupLegacy.Id };
            _context.GoodsGroups.AddRange(newGroups);

            var changedGoodGroups = from goodGroupLegacy in goodGroupsLegacy
                                    join goodGroup in goodGroups on goodGroupLegacy.Id equals goodGroup.LegacyId into t
                                    from subGroup in t.DefaultIfEmpty()
                                    where subGroup != null && subGroup.Name != goodGroupLegacy.Name
                                    select new { db = subGroup, name = goodGroupLegacy.Name };
            foreach (var changeGroup in changedGoodGroups)
                {
                    _context.Entry(changeGroup.db).State=EntityState.Modified;
                    changeGroup.db.Name = changeGroup.name;
                }
        }

        private async Task synchGoods(GoodLegacyRepository repository, int shopId)
        {
            var goodsLegacy = await repository.GetGoodsAsync();

            var groups = await _context.GoodsGroups.Where(gr => gr.ShopId == shopId).AsNoTracking().ToListAsync();
            var suppliers = await _context.Suppliers.Where(s => s.ShopId == shopId).AsNoTracking().ToListAsync();
            var goods = await _context.Goods.Include(g => g.GoodPrices).Include(g => g.Barcodes).AsNoTracking().ToListAsync();
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
                               GoodPrices= goodLegacy.GoodPrices.Select(p=>new GoodPrice { Price=p.Price, ShopId=shopId}).ToList(),
                               Barcodes = goodLegacy.Barcodes.Select(b=>new Barcode { Code=b.Code ?? "1" }).ToList(),
                               SpecialType = goodLegacy.SpecialType,
                               VPackage = goodLegacy.VPackage,
                               IsDeleted = goodLegacy.IsDeleted,
                               Uuid = goodLegacy.Uuid,
                               LegacyId = goodLegacy.Id,
                               CurrentBalances = _context.Shops.Select(s=>new GoodCurrentBalance {ShopId=s.Id }).ToList()
                           };
            _context.Goods.AddRange(newGoods);
            var changeGoods = from goodLegacy in goodsLegacy
                              join good in goods on goodLegacy.Id equals good.LegacyId into t
                              from subGood in t.DefaultIfEmpty()
                              where subGood != null && !goodCompare(subGood, goodLegacy)
                              select new
                              {
                                  db = subGood,
                                  name = goodLegacy.Name,
                                  price = goodLegacy.Price,
                                  isDeleted = goodLegacy.IsDeleted,
                                  vpackage = goodLegacy.VPackage
                              };
            foreach (var good in changeGoods)
            {
                _context.Entry(good.db);
                _context.Entry(good.db).State = EntityState.Modified;
                good.db.Name = good.name;
                good.db.Price = good.price;
                good.db.IsDeleted = good.isDeleted;
                good.db.VPackage = good.vpackage;
                foreach (var price in good.db.GoodPrices)
                {
                    _context.Entry(price).State=EntityState.Modified;
                    price.Price = good.price;
                }
            }


        }

        private bool goodCompare(Good subGood, Good goodLegacy) =>
            subGood.Name == goodLegacy.Name
            & subGood.Price == goodLegacy.Price
            & subGood.IsDeleted == goodLegacy.IsDeleted
            & subGood.VPackage == goodLegacy.VPackage;

        private void normalizeSequence()
        {
            int max = _context.Goods.Max(g => g.Id) + 1;
            string sql = $"ALTER SEQUENCE public.\"Goods_Id_seq\" RESTART WITH {max}";
            _context.Database.ExecuteSqlRaw(sql);

            max = _context.Suppliers.Max(s => s.Id) + 1;
            sql = $"ALTER SEQUENCE public.\"Suppliers_Id_seq\" RESTART WITH {max}";
            _context.Database.ExecuteSqlRaw(sql);

            max = _context.GoodsGroups.Max(s => s.Id) + 1;
            sql = $"ALTER SEQUENCE public.\"GoodsGroups_Id_seq\" RESTART WITH {max}";
            _context.Database.ExecuteSqlRaw(sql);

            max = _context.GoodPrices.Max(s => s.Id) + 1;
            sql = $"ALTER SEQUENCE public.\"GoodPrice_Id_seq\" RESTART WITH {max}";
            _context.Database.ExecuteSqlRaw(sql);

            max = _context.Barcodes.Max(s => s.Id) + 1;
            sql = $"ALTER SEQUENCE public.\"Barcodes_Id_seq\" RESTART WITH {max}";
            _context.Database.ExecuteSqlRaw(sql);
        }
    }
}
