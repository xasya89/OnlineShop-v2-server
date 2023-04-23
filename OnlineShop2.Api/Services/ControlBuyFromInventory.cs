using Microsoft.EntityFrameworkCore;
using OnlineShop2.Database;
using OnlineShop2.Database.Models;
using OnlineShop2.LegacyDb;
using OnlineShop2.LegacyDb.Models;
using OnlineShop2.LegacyDb.Repositories;
using System.Threading;

namespace OnlineShop2.Api.Services
{
    public class ControlBuyFromInventoryBackgroundService : IHostedService, IDisposable
    {
        private readonly ILogger<ControlBuyFromInventoryBackgroundService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _service;
        private readonly IUnitOfWorkLegacy _unitOfWork;
        private readonly string inventoryShema;
        private Timer? _timer = null;
        public ControlBuyFromInventoryBackgroundService(
            ILogger<ControlBuyFromInventoryBackgroundService> logger, 
            IServiceProvider service,
            IConfiguration configuration,
            IUnitOfWorkLegacy unitOfWork)
        {
            _logger = logger;
            _service = service;
            _configuration = configuration;
            _unitOfWork = unitOfWork;
            inventoryShema = configuration.GetSection("InventoryShema").Value;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            int period =_configuration.GetSection("Cron").GetValue<int>("ControlBuyFromInventory");
            _timer = new Timer(DoWork, null, 0, period);
            return Task.CompletedTask;
        }

        private async void DoWork(object? state)
        {
            try
            {
                using var scope = _service.CreateScope();
                using var context = scope.ServiceProvider.GetRequiredService<OnlineShopContext>();
                var inventories = await context.Inventories
                    .Where(i => i.Status!=DocumentStatus.Successed).AsNoTracking().ToListAsync();
                //TODO: Заменить foreach на Expression values
                foreach (var inventory in inventories)
                {
                    if (inventory.Status == DocumentStatus.New || inventory.Status == DocumentStatus.Processing)
                        await calcChack(context, inventory);
                    if (inventory.Status == DocumentStatus.Processing)
                        await calcCountDiif(context, inventory);

                };
                await context.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                _logger.LogError($"HostedService - ControlBuyFromInventory \n " + ex.Message);
            }
            
        }

        /// <summary>
        /// Определение чеков во время проведения инвенторизации
        /// выполняем вычитание если товар указан в инвенторизации
        /// </summary>
        /// <param name="context"></param>
        /// <param name="inventory"></param>
        /// <returns></returns>
        private async Task calcChack(OnlineShopContext context, Inventory inventory)
        {
            var inventoryAppendedChecksId = await context.InventoryAppendChecks
                .Where(c=>c.InventoryId==inventory.Id).GroupBy(c=>c.CheckSellId).AsNoTracking().Select(c=>c.Key).ToListAsync();

            var checks = await context.CheckSells.Include(c => c.CheckGoods).Include(c => c.Shift)
                        .Where(c => 
                            !inventoryAppendedChecksId.Contains(c.Id) & 
                            c.ShiftId>=inventory.CurrentShiftId & 
                            c.Shift.ShopId==inventory.ShopId &
                            c.DateCreate > inventory.Start)
                        .AsNoTracking().ToListAsync();
            if (checks.Count == 0) return;
            var goodsIdInInventory = (await context.InventoryGroups.Include(g => g.InventoryGoods)
                .Where(g => g.InventoryId == inventory.Id).AsNoTracking().ToListAsync())
                .SelectMany(g => g.InventoryGoods).GroupBy(g => g.GoodId).Select(g=>g.Key);
            context.InventoryAppendChecks.AddRange(
                checks.SelectMany(c => c.CheckGoods)
                .Where(c => !goodsIdInInventory.Contains(c.GoodId))
                .Select(c =>
                new InventoryAppendCheck
                {
                    InventoryId = inventory.Id,
                    ShopId = inventory.ShopId,
                    CheckSellId = c.CheckSellId,
                    GoodId = c.GoodId,
                    Count = -1 * c.Count
                }));
        }

        /// <summary>
        /// Расчет расхождений после завершения инвенторизации
        /// и закрытия смены
        /// </summary>
        /// <param name="context"></param>
        /// <param name="inventory"></param>
        /// <returns></returns>
        private async Task calcCountDiif(OnlineShopContext context, Inventory inventory)
        {
            if (inventoryShema == "CurrentBalanceOnStart") return;
            if (inventory.CurrentShiftId == null) return;
            var curShift = await context.Shifts.Where(s=>s.Id>=inventory.CurrentShiftId & s.ShopId==inventory.ShopId & s.Stop!=null).FirstOrDefaultAsync();
            if(curShift==null) return;

            var inventoryGoods = await context.InventoryGroups.Include(i => i.InventoryGoods).Where(i => i.InventoryId == inventory.Id)
                .SelectMany(x=>x.InventoryGoods).GroupBy(x=>x.GoodId).Select(x=> new { GoodId = x.Key, countFact = x.Sum(g => g.CountFact) })
                .ToArrayAsync();

            var currentBalance = await context.GoodCurrentBalances.Include(x=>x.Good)
                .ThenInclude(x=>x.GoodPrices.Where(p=>p.ShopId==inventory.ShopId))
                .Where(x => x.ShopId == inventory.ShopId & !x.Good.IsDeleted).AsNoTracking().ToArrayAsync();

            //Вычтем продажи за время инвенторизации
            var checkGoods = await context.InventoryAppendChecks.Where(x => x.InventoryId == inventory.Id).GroupBy(x => x.GoodId)
                .AsNoTracking().Select(x => new { goodId = x.Key, count = x.Sum(x => x.Count) }).ToArrayAsync();

            var countSummaries = from balance in currentBalance
                                 join inventoryGood in inventoryGoods on balance.GoodId equals inventoryGood.GoodId into t
                                 from sub in t.DefaultIfEmpty()
                                 select new {
                                     GoodLegacyId=balance.Good.LegacyId,
                                     InventoryId = inventory.Id,
                                     GoodId = balance.GoodId,
                                     CountOld = balance.CurrentCount,
                                     CountCurrent = (sub?.countFact ?? 0) + (checkGoods.Where(x=>x.goodId==balance.GoodId).FirstOrDefault()?.count ?? 0),
                                     Price = balance.Good.GoodPrices.First().Price
                                 };

            var transaction = context.Database.BeginTransaction();
            context.InventorySummaryGoods.AddRange(countSummaries.Select(x=>new InventorySummaryGood {
                InventoryId=x.InventoryId,
                GoodId=x.GoodId,
                CountOld=x.CountOld,
                CountCurrent=x.CountCurrent,
                Price=x.Price
            }));

            inventory.SumDb = countSummaries.Sum(x => x.CountOld * x.Price);
            inventory.SumFact = countSummaries.Sum(x => x.CountCurrent * x.Price);
            inventory.Status = DocumentStatus.Complited;
            context.Entry(inventory).State = EntityState.Modified;

            var changedBalance = from b in currentBalance
                                 join c in countSummaries on b.GoodId equals c.GoodId
                                 where b.CurrentCount != c.CountCurrent
                                 select new { db = b, count = c.CountCurrent };
            foreach(var change in changedBalance)
            {
                context.Entry(change.db).State = EntityState.Modified;
                change.db.CurrentCount = change.count;
            }

            var shop = context.Shops.Find(inventory.ShopId);
            if(shop!=null && shop.LegacyDbNum!=null)
            {
                _unitOfWork.SetConnectionString(_configuration.GetConnectionString("shop" + shop.LegacyDbNum));
                _unitOfWork.CurrentBalance.SetCurrent(countSummaries.Where(x => x.GoodLegacyId != null)
                    .Select(x => new GoodCountBalanceCurrentLegacy { GoodId = x.GoodLegacyId ?? 0, Count = x.CountCurrent }).ToList());
            }

            context.SaveChanges();

            transaction.Commit();
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
    }
}
