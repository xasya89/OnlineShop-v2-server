using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using OnlineShop2.Database;
using OnlineShop2.LegacyDb;
using System.Threading;

namespace OnlineShop2.Api.Services.Legacy
{
    public class ShiftSynchBackgroundService: IHostedService, IDisposable
    {
        private readonly ILogger<ShiftSynchBackgroundService> _logger;
        private System.Threading.Timer? _timer = null;
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _service;

        public ShiftSynchBackgroundService(ILogger<ShiftSynchBackgroundService> logger, IConfiguration configuration, IServiceProvider service)
        {
            _logger = logger;
            _configuration = configuration;
            _service = service;
        }

        private async void DoWork(object? state)
        {
            using var scope = _service.CreateScope();
            var synchService = scope.ServiceProvider.GetRequiredService<SynchLegacyService>();
            
            using var context = scope.ServiceProvider.GetRequiredService<OnlineShopContext>();
            try
            {
                var shops = await context.Shops.Where(s => s.LegacyDbNum != null).ToListAsync();
                foreach (var shop in shops)
                {
                    string constr = _configuration.GetConnectionString("shop" + shop.LegacyDbNum);
                    if (constr == null)
                        return;
                    await synchService.SynchGoods(shop.Id, shop.LegacyDbNum??0);
                    await new UnitOfWorkLegacy(constr).ShiftLegacyRepository.ShiftSynch(context, DateOnly.FromDateTime(DateTime.Now), shop.Id);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("HostedService - ShiftSynch \n" + ex.Message);
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            int period = _configuration.GetSection("Cron").GetValue<int>("ShiftSynch");
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
    }
}
