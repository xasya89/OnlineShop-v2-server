using Microsoft.EntityFrameworkCore;
using OnlineShop2.Database;
using OnlineShop2.Database.Models;

namespace OnlineShop2.Api.Services.HostedService.MoneyReportMesssageHostService
{
    /// <summary>
    /// Сервис создает в 00:00 новый отчет и переносит остатки на начало
    /// </summary>
    public class MoneyReportDailyCreate : IHostedService, IDisposable
    {
        private System.Threading.Timer? _timer = null;
        private readonly IServiceProvider _service;
        private readonly IConfiguration _configuration;
        private readonly ILogger<MoneyReportDailyCreate> _logger;
        private int period;

        public MoneyReportDailyCreate(IServiceProvider service, IConfiguration configuration, ILogger<MoneyReportDailyCreate> logger)
        {
            _service = service;
            _configuration = configuration;
            _logger = logger;
            period = _configuration.GetValue<int>("Cron:ShiftSynch");
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(DoWork, null, 0, period);
            return Task.CompletedTask;
        }

        bool calcNowFlag = false;
        private async void DoWork(object? state)
        {
            if(TimeOnly.FromDateTime(DateTime.Now).Hour!=0)
                calcNowFlag = false;
            if(!calcNowFlag && TimeOnly.FromDateTime(DateTime.Now).Hour==00)
                try
                {
                    using var scope = _service.CreateScope();
                    using var context = scope.ServiceProvider.GetRequiredService<OnlineShopContext>();
                    DateTime withoutTime = DateOnly.FromDateTime(DateTime.Now).ToDateTime(TimeOnly.MinValue);
                    var report = await context.MoneyReports.Where(x => DateTime.Compare(x.Create, withoutTime) == 0).AsNoTracking().FirstOrDefaultAsync();
                    if (report != null)
                    {
                        calcNowFlag = true;
                        return;
                    }

                    //Вчерашний отчет
                    DateTime yesterday = DateOnly.FromDateTime(DateTime.Now).ToDateTime(TimeOnly.MinValue).AddDays(-1);
                    var prevReport = await context.MoneyReports.Where(x=>DateTime.Compare(x.Create, yesterday)==0).AsNoTracking().FirstOrDefaultAsync();

                    context.MoneyReports.Add(new MoneyReport 
                    { 
                        Create = withoutTime, 
                        StartGoodSum = prevReport?.StopGoodSum ?? 0,
                        StartCashMoney = prevReport?.MoneyItog ?? 0
                    });
                    await context.SaveChangesAsync();
                }
                catch(Exception ex)
                {
                    _logger.LogError("MoneyReportDailyCreate ошибка: "+ex.Message);
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
