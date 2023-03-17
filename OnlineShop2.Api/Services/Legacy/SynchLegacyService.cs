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

            var suppliers = _context.Suppliers;
            var newSuppliers = from supplierLegacy in suppliersLegacy
                               join supplier in suppliers on supplierLegacy.Id equals supplier.Id into t
                               from subSupplier in t.DefaultIfEmpty()
                               where subSupplier == null
                               select supplierLegacy;
            _context.Suppliers.AddRange(newSuppliers);
            foreach (var supplier in newSuppliers)
                supplier.ShopId = shopId;

            var changedSuppliers = from supplierLegacy in suppliersLegacy
                                   join supplier in suppliers on supplierLegacy.Id equals supplier.Id into t
                                   from subSupplier in t.DefaultIfEmpty()
                                   where subSupplier != null && subSupplier.Name != supplierLegacy.Name + " "
                                   select new { db = subSupplier, name = supplierLegacy.Name };
            foreach (var changeSupplier in changedSuppliers)
                changeSupplier.db.Name = changeSupplier.name;
        }

        private async Task synchGoodGroups(GoodLegacyRepository repository, int shopId)
        {
            var goodGroupsLegacy = await repository.GetGroupsAsync();

            var goodGroups = _context.GoodsGroups;
            var newGroups = from goodGroupLegacy in goodGroupsLegacy
                            join goodGroup in goodGroups on goodGroupLegacy.Id equals goodGroup.Id into t
                            from subGroup in t.DefaultIfEmpty()
                            where subGroup == null
                            select goodGroupLegacy;
            foreach (var goodGroup in newGroups)
                goodGroup.ShopId = shopId;
            _context.GoodsGroups.AddRange(newGroups);

            var changedGoodGroups = from goodGroupLegacy in goodGroupsLegacy
                                    join goodGroup in goodGroups on goodGroupLegacy.Id equals goodGroup.Id into t
                                    from subGroup in t.DefaultIfEmpty()
                                    where subGroup != null && subGroup.Name != goodGroupLegacy.Name
                                    select new { db = subGroup, name = goodGroupLegacy.Name };
            foreach (var changeGroup in changedGoodGroups)
                changeGroup.db.Name = changeGroup.name;
        }

        private async Task synchGoods(GoodLegacyRepository repository, int shopId)
        {
            var goodsLegacy = await repository.GetGoodsAsync();

            var goods = _context.Goods.Include(g => g.GoodPrices).Include(g => g.Barcodes);
            var newGoods = from goodLegacy in goodsLegacy
                           join good in goods on goodLegacy.Id equals good.Id into t
                           from subGood in t.DefaultIfEmpty()
                           where subGood == null
                           select goodLegacy;
            foreach (var good in newGoods)
            {
                good.ShopId = shopId;
                foreach (var price in good.GoodPrices)
                    price.ShopId = shopId;
                foreach (var barcode in good.Barcodes.Where(b => b.Code == null))
                    barcode.Code = "1";
            }
            _context.Goods.AddRange(newGoods);
            var changeGoods = from goodLegacy in goodsLegacy
                              join good in goods on goodLegacy.Id equals good.Id into t
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
                good.db.Name = good.name;
                good.db.Price = good.price;
                good.db.IsDeleted = good.isDeleted;
                good.db.VPackage = good.vpackage;
                foreach (var price in good.db.GoodPrices)
                    price.Price = good.price;
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
