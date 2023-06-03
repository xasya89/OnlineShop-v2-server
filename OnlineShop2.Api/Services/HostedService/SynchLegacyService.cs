using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OnlineShop2.Api.Services.HostedService.SynchMethods;
using OnlineShop2.Api.Services.Legacy;
using OnlineShop2.Database;
using OnlineShop2.Database.Migrations;
using OnlineShop2.Database.Models;
using OnlineShop2.LegacyDb;
using OnlineShop2.LegacyDb.Models;
using OnlineShop2.LegacyDb.Repositories;

namespace OnlineShop2.Api.Services.HostedService
{
    public class SynchLegacyHostedService : IHostedService, IDisposable
    {
        private System.Threading.Timer? _timer = null;
        private readonly ILogger<SynchLegacyHostedService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _service;
        private readonly IMapper _mapper;

        public SynchLegacyHostedService(ILogger<SynchLegacyHostedService> logger, IConfiguration configuration, IServiceProvider service, IMapper mapper)
        {
            _logger = logger;
            _configuration = configuration;
            _service = service;
            _mapper = mapper;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            int period = _configuration.GetValue<int>("Cron:ShiftSynch");
            _timer = new Timer(DoWork, null, 0, period);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }
        public void Dispose()
        {
            _timer?.Dispose();
        }

        private async void DoWork(object? state)
        {
            using var scope = _service.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWorkLegacy>();
            using var context = scope.ServiceProvider.GetRequiredService<OnlineShopContext>();
            var moneyReportChannelService = scope.ServiceProvider.GetRequiredService<MoneyReportChannelService>();
            try
            {
                var shops = await context.Shops.Where(s => s.LegacyDbNum != null).ToListAsync();
                foreach (var shop in shops)
                {
                    string constr = _configuration.GetConnectionString("shop" + shop.LegacyDbNum);
                    if (constr == null)
                        return;
                    unitOfWork.SetConnectionString(constr);

                    await RevaluationSynch.Synch(context, unitOfWork.RevaluationRepositoryLegacy, shop.Id);

                    await synchSuppliers((OnlineShopContext)context, (IUnitOfWorkLegacy)unitOfWork, shop.Id);
                    await goodGroupSynch((OnlineShopContext)context, (IUnitOfWorkLegacy)unitOfWork, shop.Id);
                    await goodSynch((OnlineShopContext)context, (IUnitOfWorkLegacy)unitOfWork, shop.Id);
                    await synchSuppliers((OnlineShopContext)context, (IUnitOfWorkLegacy)unitOfWork, shop.Id);
                    await ShiftSynch.StartSynch(context, unitOfWork, shop.Id, moneyReportChannelService);
                    await ArrivalSynch.StartSynch((OnlineShopContext)context, (IUnitOfWorkLegacy)unitOfWork, shop.Id, moneyReportChannelService);
                    await WriteOfSynch.StartSync((OnlineShopContext)context, _mapper, (IUnitOfWorkLegacy)unitOfWork, shop.Id, moneyReportChannelService);

                }
            }
            catch (Exception ex)
            {
                _logger.LogError("HostedService - ShiftSynch \n" + ex.Message + "\n\n" + ex.StackTrace);
            }
        }

        private async Task synchSuppliers(OnlineShopContext context, IUnitOfWorkLegacy unitOfWork, int shopId)
        {

            var suppliersLegacy = await unitOfWork.SupplierRepository.GetAllAsync();

            var suppliers = await context.Suppliers.AsNoTracking().ToListAsync();
            var newSuppliers = from supplierLegacy in suppliersLegacy
                               join supplier in suppliers on supplierLegacy.Id equals supplier.LegacyId into t
                               from subSupplier in t.DefaultIfEmpty()
            where subSupplier == null
                               select new Supplier { Name = supplierLegacy.Name, ShopId = shopId, LegacyId = supplierLegacy.Id };
            context.Suppliers.AddRange(newSuppliers);

            var changedSuppliers = from supplierLegacy in suppliersLegacy
                                   join supplier in suppliers on supplierLegacy.Id equals supplier.LegacyId into t
                                   from subSupplier in t.DefaultIfEmpty()
                                   where subSupplier != null && subSupplier.Name != supplierLegacy.Name
                                   select new { db = subSupplier, name = supplierLegacy.Name };
            foreach (var changeSupplier in changedSuppliers)
            {
                context.Entry(changeSupplier.db).State = EntityState.Modified;
                changeSupplier.db.Name = changeSupplier.name;
            }

            await context.SaveChangesAsync();
            foreach (var supplier in newSuppliers)
                context.Entry(supplier).State = EntityState.Detached;
            foreach (var supplier in newSuppliers)
                context.Entry(supplier).State = EntityState.Detached;
        }

        private async Task goodGroupSynch(OnlineShopContext context, IUnitOfWorkLegacy unitOfWork, int shopId)
        {
            var goodGroupsLegacy = await unitOfWork.GoodGroupRepository.GetAllAsync();

            var goodGroups = await context.GoodsGroups.AsNoTracking().ToListAsync();
            var newGroups = from goodGroupLegacy in goodGroupsLegacy
                            join goodGroup in goodGroups on goodGroupLegacy.Id equals goodGroup.LegacyId into t
                            from subGroup in t.DefaultIfEmpty()
                            where subGroup == null
                            select new GoodGroup { ShopId = shopId, Name = goodGroupLegacy.Name, LegacyId = goodGroupLegacy.Id };
            context.GoodsGroups.AddRange(newGroups);

            var changedGoodGroups = from goodGroupLegacy in goodGroupsLegacy
                                    join goodGroup in goodGroups on goodGroupLegacy.Id equals goodGroup.LegacyId into t
                                    from subGroup in t.DefaultIfEmpty()
                                    where subGroup != null && subGroup.Name != goodGroupLegacy.Name
                                    select new { db = subGroup, name = goodGroupLegacy.Name };
            foreach (var changeGroup in changedGoodGroups)
            {
                context.Entry(changeGroup.db).State = EntityState.Modified;
                changeGroup.db.Name = changeGroup.name;
            }
            await context.SaveChangesAsync();

            foreach (var group in newGroups)
                context.Entry(group).State = EntityState.Detached; 
            foreach (var group in changedGoodGroups)
                context.Entry(group).State = EntityState.Detached;
        }

        private async Task goodSynch(OnlineShopContext context, IUnitOfWorkLegacy unitOfWork, int shopId)
        {
            var goodsLegacy = await unitOfWork.GoodRepository.GetAllAsync();

            var groups = await context.GoodsGroups.Where(gr => gr.ShopId == shopId).AsNoTracking().ToListAsync();
            var suppliers = await context.Suppliers.Where(s => s.ShopId == shopId).AsNoTracking().ToListAsync();
            var goods = await context.Goods.Include(g => g.GoodPrices).Include(g => g.Barcodes).AsNoTracking().ToListAsync();
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
                context.Entry(good.db);
                context.Entry(good.db).State = EntityState.Modified;
                good.db.Name = good.name;
                good.db.Price = good.price;
                good.db.IsDeleted = good.isDeleted;
                good.db.VPackage = good.vpackage;
                foreach (var price in good.db.GoodPrices)
                {
                    context.Entry(price).State = EntityState.Modified;
                    price.Price = good.price;
                }
            }

            await context.SaveChangesAsync();

            foreach (var good in newGoods)
                context.Entry(good).State = EntityState.Detached;
            foreach (var good in changeGoods)
                context.Entry(good).State = EntityState.Detached;
        }

        private bool goodCompare(Good subGood, GoodLegacy goodLegacy) =>
            subGood.Name == goodLegacy.Name
            & subGood.Price == goodLegacy.Price
            & subGood.IsDeleted == goodLegacy.IsDeleted
            & subGood.VPackage == goodLegacy.VPackage;
    }
}
