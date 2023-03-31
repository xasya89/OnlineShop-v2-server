using Microsoft.EntityFrameworkCore;
using OnlineShop2.Database;
using OnlineShop2.Database.Models;
using System.Threading;

namespace OnlineShop2.Api.Services
{
    public class ControlBuyFromInventoryBackgroundService : IHostedService, IDisposable
    {
        private readonly ILogger<ControlBuyFromInventoryBackgroundService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _service;
        private Timer? _timer = null;
        public ControlBuyFromInventoryBackgroundService(ILogger<ControlBuyFromInventoryBackgroundService> logger, IServiceProvider service, IConfiguration configuration)
        {
            _logger = logger;
            _service = service;
            _configuration = configuration;
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
                var inventories = await context.Inventories.Include(i => i.InventoryAppendChecks).Where(i => i.Stop == null).AsNoTracking().ToListAsync();
                var inventoryAppendedChecksId = inventories.SelectMany(i => i.InventoryAppendChecks).Select(c => c.CheckSellId);
                //TODO: Заменить foreach на Expression values
                foreach (var inventory in inventories)
                {
                    var checks = await context.CheckSells.Include(c => c.CheckGoods).Include(c => c.Shift)
                        .Where(c => !inventoryAppendedChecksId.Contains(c.Id) & c.DateCreate >= inventory.Start & c.Shift.ShopId == inventory.ShopId)
                        .AsNoTracking().ToListAsync();
                    var goodsInInventory = (await context.InventoryGroups.Include(g => g.InventoryGoods)
                        .Where(g => g.InventoryId == inventory.Id).AsNoTracking().ToListAsync())
                        .SelectMany(g => g.InventoryGoods).GroupBy(g => g.GoodId);

                    context.InventoryAppendChecks.AddRange(
                        checks.SelectMany(c => c.CheckGoods).Select(c =>
                        new InventoryAppendCheck
                        {
                            InventoryId = inventory.Id,
                            ShopId = inventory.ShopId,
                            CheckSellId = c.CheckSellId,
                            GoodId = c.GoodId,
                            Count = (goodsInInventory.Where(g => g.Key == c.GoodId).FirstOrDefault() == null ? 1 : -1) * c.Count
                        }));
                };
                await context.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                _logger.LogError($"HostedService - ControlBuyFromInventory \n " + ex.Message);
            }
            
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
