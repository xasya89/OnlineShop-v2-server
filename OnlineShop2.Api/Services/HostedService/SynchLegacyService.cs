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
                    await shiftSynch((OnlineShopContext)context, (IUnitOfWorkLegacy)unitOfWork, shop.Id);
                    await arrivalSynch((OnlineShopContext)context, (IUnitOfWorkLegacy)unitOfWork, shop.Id);
                    await writeofSynch((OnlineShopContext)context, (IUnitOfWorkLegacy)unitOfWork, shop.Id);

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

        private async Task shiftSynch(OnlineShopContext context, IUnitOfWorkLegacy unitOfWork, int shopId)
        {
            DateOnly with = DateOnly.FromDateTime(DateTime.Now.AddDays(-1));
            var shiftsLegacy = await unitOfWork.ShiftRepository.GetShifts(with);
            IEnumerable<int?> shiftsLegacyId = Array.ConvertAll(shiftsLegacy.Select(s => s.Id).ToArray(), value => new int?(value));
            var shifts = await context.Shifts.Include(s => s.CheckSells).ThenInclude(c => c.CheckGoods).Include(s => s.ShiftSummaries)
                .Where(s => shiftsLegacyId.Contains(s.LegacyId)).AsNoTracking().ToListAsync();
            //Получаем список товаров в чеках 
            var goodsLegacyId = shiftsLegacy.SelectMany(s => s.CheckSells).SelectMany(c => c.CheckGoods).GroupBy(g => g.GoodId).Select(c => c.Key);
            IEnumerable<int?> goodsLegacyIdNullable = Array.ConvertAll(goodsLegacyId.ToArray(), val => new int?(val));
            var goods = await context.Goods.Where(g => goodsLegacyIdNullable.Contains(g.LegacyId)).AsNoTracking().ToListAsync();

            var newShifts = from legacy in shiftsLegacy
                            join shift in shifts on legacy.Id equals shift.LegacyId into t
                            from sub in t.DefaultIfEmpty()
                            where sub == null
                            select new Shift
                            {
                                Start = legacy.Start,
                                Stop = legacy.Stop,
                                ShopId = shopId,
                                Uuid = legacy.Uuid,
                                SumAll = legacy.SumAll,
                                SumElectron = legacy.SumElectron,
                                SumNoElectron = legacy.SumNoElectron,
                                SumSell = legacy.SumSell,
                                SumDiscount = legacy.SumDiscount,
                                SumReturnNoElectron = legacy.SumReturnNoElectron,
                                SumReturnElectron = legacy.SumReturnElectron,
                                LegacyId = legacy.Id,
                                CheckSells = legacy.CheckSells.Select(c => new CheckSell
                                {
                                    DateCreate = c.DateCreate,
                                    TypeSell = c.TypeSell,
                                    SumBuy = legacy.SumAll,
                                    SumDiscont = c.SumDiscont,
                                    SumElectron = c.SumElectron,
                                    SumNoElectron = c.SumCash,
                                    LegacyId = c.Id,
                                    CheckGoods = c.CheckGoods.Select(c => new CheckGood
                                    {
                                        GoodId = goods.Where(g => g.LegacyId == c.GoodId).First().Id,
                                        Count = c.Count,
                                        Price = c.Price
                                    }).ToList()
                                }).ToList()
                            };
            context.AddRange(newShifts);

            var editingShifts = from legacy in shiftsLegacy
                                join shift in shifts on legacy.Id equals shift.LegacyId into t
                                from sub in t.DefaultIfEmpty()
                                where sub != null
                                select new { db = sub, legacy = legacy };
            foreach (var row in editingShifts)
            {
                context.Entry(row.db).State = EntityState.Unchanged;
                var prop = context.Entry(row.db).Property(x => x.Start);
                row.db.Stop = row.legacy.Stop;
                row.db.SumAll = row.legacy.SumAll;
                row.db.SumElectron = row.legacy.SumElectron;
                row.db.SumNoElectron = row.legacy.SumNoElectron;
                row.db.SumSell = row.legacy.SumSell;
                row.db.SumDiscount = row.legacy.SumDiscount;
                row.db.SumReturnNoElectron = row.legacy.SumReturnNoElectron;
                row.db.SumReturnElectron = row.legacy.SumReturnElectron;
            }

            //Получим новые чеки
            var checksLegacy = shiftsLegacy.Where(s => !newShifts.Any(x => x.LegacyId == s.Id)).SelectMany(s => s.CheckSells);
            var checks = shifts.SelectMany(s => s.CheckSells).ToList();
            var newChecks = from legacy in checksLegacy
                            join check in checks on legacy.Id equals check.LegacyId into t
                            from sub in t.DefaultIfEmpty()
                            where sub == null
                            select new CheckSell
                            {
                                Shift = shifts.Where(s => s.LegacyId == legacy.ShiftId).FirstOrDefault() ?? newShifts.Where(s => s.LegacyId == legacy.ShiftId).First(),
                                DateCreate = legacy.DateCreate,
                                TypeSell = legacy.TypeSell,
                                SumBuy = legacy.SumAll,
                                SumDiscont = legacy.SumDiscont,
                                SumElectron = legacy.SumElectron,
                                SumNoElectron = legacy.SumCash,
                                CheckGoods = legacy.CheckGoods.Select(c => new CheckGood
                                {
                                    GoodId = goods.Where(g => g.LegacyId == c.GoodId).First().Id,
                                    Count = c.Count,
                                    Price = c.Price
                                }).ToList(),
                                LegacyId = legacy.Id
                            };
            context.AddRange(newChecks);

            await context.SaveChangesAsync();

            foreach (var shift in newShifts)
                context.Entry(shift).State = EntityState.Detached;
            foreach (var check in newChecks)
                context.Entry(check).State = EntityState.Detached;
        }

        private async Task arrivalSynch(OnlineShopContext context, IUnitOfWorkLegacy unitOfWork, int shopId)
        {
            DateTime with = DateOnly.FromDateTime(DateTime.Now).ToDateTime(TimeOnly.MinValue);
            var laegacyArrivals = await unitOfWork.ArrivalRepository.GetArrivalWithDate(with);
            
            var arrivals = await context.Arrivals.Where(a => a.DateArrival >= with).ToListAsync();
            var arrivalLegacyIds = arrivals.Select(a => a.LegacyId);

            var suppliers = await context.Suppliers.AsNoTracking().ToListAsync();
            var goods = await context.Goods.AsNoTracking().ToListAsync();
            var newArrivals = laegacyArrivals.Where(a => !arrivalLegacyIds.Contains(a.Id)).Select(a=>
                new Arrival
                {
                    Id=0,
                    Num=a.Num,
                    DateArrival=a.DateArrival,
                    SupplierId = suppliers.Find(s=>s.LegacyId==a.SupplierId).Id,
                    ShopId=shopId,
                    Status=DocumentStatus.Successed,
                    PurchaseAmount = a.SumArrival,
                    SaleAmount=a.SumSell,
                    SumNds=a.SumNds,
                    LegacyId=a.Id,
                    ArrivalGoods = a.ArrivalGoods.Select(x=>new ArrivalGood
                    {
                        Id=0,
                        ArrivalId=0,
                        GoodId = goods.Find(g=>g.LegacyId==x.GoodId).Id,
                        Count=x.Count,
                        PricePurchase=x.Price,
                        PriceSell=x.PriceSell,
                        Nds=NDSType.None,
                        ExpiresDate=x.ExpiresDate
                    }).ToList()
                }
            );
            if (newArrivals.Count() == 0) return; 
            context.Arrivals.AddRange(newArrivals);

            //Увеличим кол-во
            var changeGoodsBalance = newArrivals.SelectMany(a => a.ArrivalGoods).GroupBy(x => x.GoodId).Select(x => new { GoodId = x.Key, Count = x.Sum(x => x.Count) });
            foreach (var change in changeGoodsBalance)
                await context.GoodCurrentBalances
                    .Where(x => x.GoodId == change.GoodId)
                    .ExecuteUpdateAsync(x => x.SetProperty(x => x.CurrentCount, x => x.CurrentCount + change.Count));

            await context.SaveChangesAsync();

            foreach (var newArrival in newArrivals)
                context.Entry(newArrival).State = EntityState.Detached;
        }

        private async Task writeofSynch(OnlineShopContext context, IUnitOfWorkLegacy unitOfWork, int shopId)
        {
            DateTime with = DateOnly.FromDateTime(DateTime.Now).ToDateTime(TimeOnly.MinValue);
            var legacyList = _mapper.Map<IEnumerable<Writeof>> ( await unitOfWork.WriteofRepositoryLegacy.GetWriteOfWithDate(DateTime.Now));
            var legacyInDbIds = await context.Writeofs.Where(w => w.DateWriteof >= with).AsNoTracking().Select(w=>w.LegacyId).ToListAsync();
            
            var legacyGoodsIds = legacyList.SelectMany(w => w.WriteofGoods).GroupBy(x => x.GoodId).Select(x => x.Key).ToList();
            var goods = await context.Goods.Where(g => legacyGoodsIds.Contains(g.LegacyId ?? 0)).AsNoTracking().ToListAsync();

            foreach(var legacy in legacyList.Where(w=>!legacyInDbIds.Contains(w.Id)))
            {
                legacy.LegacyId = legacy.Id;
                legacy.Id = 0;
                legacy.ShopId = shopId;
                legacy.WriteofGoods.ForEach(g =>
                {
                    g.Id = 0;
                    g.WriteofId = 0;
                    g.GoodId = goods.Where(x => x.LegacyId == g.GoodId).First().Id;

                    context.GoodCurrentBalances.Where(x => x.ShopId == shopId & x.GoodId == g.GoodId)
                    .ExecuteUpdateAsync(x => x.SetProperty(x => x.CurrentCount, x => x.CurrentCount - g.Count));
                });
                context.Writeofs.Add(legacy);
                
            }

            await context.SaveChangesAsync();
        }
    }
}
