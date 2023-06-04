using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OnlineShop2.Api.Services.HostedService.MoneyReportMesssage.BixLogic;
using OnlineShop2.Database;
using OnlineShop2.LegacyDb.Repositories;

namespace OnlineShop2.Api.Services.HostedService.MoneyReportMesssage
{
    public class MoneyReportHostedService : BackgroundService
    {
        private readonly ILogger<MoneyReportHostedService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly MoneyReportChannelService _moneyReportChannelService;
        private Timer? _timer = null;

        public MoneyReportHostedService(ILogger<MoneyReportHostedService> logger,
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            IMapper mapper,
            MoneyReportChannelService moneyReportChannelService
            )
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _configuration = configuration;
            _mapper = mapper;
            _moneyReportChannelService = moneyReportChannelService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var message = await _moneyReportChannelService.PullAsync(stoppingToken);
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    using var context = scope.ServiceProvider.GetRequiredService<OnlineShopContext>();

                    var report = await FindMoneyReport.GetReport(context, message.Date, message.ShopId);

                    if (message.TypeDoc == Models.ReportMessage.MoneyReportMessageTypeDoc.InventoryComplite)
                    {
                        var inventory = await context.Inventories.Where(x => x.Id == message.DocId).AsNoTracking().FirstOrDefaultAsync();
                        if (inventory == null)
                            throw new Exception($"Инвенторизация id {message.DocId} не найдена");
                        report.InventoryGoodsSum = inventory.SumFact;
                        report.InventoryCashMoney = inventory.CashMoneyFact;
                    }

                    if (message.TypeDoc == Models.ReportMessage.MoneyReportMessageTypeDoc.Arrival)
                        report.ArrivalsSum += message.Sum;

                    if (message.TypeDoc == Models.ReportMessage.MoneyReportMessageTypeDoc.CashIncome)
                        report.CashIncome += message.Sum;

                    if (message.TypeDoc == Models.ReportMessage.MoneyReportMessageTypeDoc.CashOutcome)
                        report.CashOutcome += message.Sum;

                    if (message.TypeDoc == Models.ReportMessage.MoneyReportMessageTypeDoc.CheckMoney)
                        report.CashMoney += message.Sum;

                    if (message.TypeDoc == Models.ReportMessage.MoneyReportMessageTypeDoc.CheckElectron)
                        report.CashElectron += message.Sum;

                    if (message.TypeDoc == Models.ReportMessage.MoneyReportMessageTypeDoc.WriteOf)
                        report.Writeof += message.Sum;

                    if (message.TypeDoc == Models.ReportMessage.MoneyReportMessageTypeDoc.RevaluationOld)
                        report.RevaluationOld += message.Sum;

                    if (message.TypeDoc == Models.ReportMessage.MoneyReportMessageTypeDoc.RevaluationNew)
                        report.RevaluationNew += message.Sum;

                    if (message.TypeDoc == Models.ReportMessage.MoneyReportMessageTypeDoc.StopShift)
                        report.StopGoodSum = await context.GoodCurrentBalances.Include(x => x.Good).ThenInclude(x => x.GoodPrices.Where(x => x.ShopId == message.ShopId))
                            .Where(x => x.ShopId == message.ShopId)
                            .SumAsync(x => x.CurrentCount * x.Good.GoodPrices.First().Price);

                    await context.SaveChangesAsync();
                }
                catch(Exception ex)
                {
                    _logger.LogError("MoneyReportHostedService ошибка " + ex.Message+"\n message: "+message.ToString());
                }
            }
        }
    }
}
