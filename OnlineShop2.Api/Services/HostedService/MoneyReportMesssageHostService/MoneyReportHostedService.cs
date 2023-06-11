using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OnlineShop2.Api.Services.HostedService.MoneyReportMesssageHostService.BizLogic;
using OnlineShop2.Database;
using OnlineShop2.Dao;
using OnlineShop2.LegacyDb.Repositories;
using OnlineShop2.Database.Models;
using OnlineShop2.Api.Models.ReportMessage;
using System.Xml;

namespace OnlineShop2.Api.Services.HostedService.MoneyReportMesssageHostService
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
                using var scope = _serviceProvider.CreateScope();
                using var context = scope.ServiceProvider.GetRequiredService<OnlineShopContext>();
                try
                {
                    var report = await FindMoneyReport.GetReport(context, message);

                    if (message.TypeDoc == MoneyReportMessageTypeDoc.InventoryComplite)
                    {
                        var inventory = await context.Inventories.Where(x => x.Id == message.DocId).AsNoTracking().FirstOrDefaultAsync();
                        if (inventory == null)
                            throw new Exception($"Инвенторизация id {message.DocId} не найдена");
                        report.InventoryGoodsSum = inventory.SumFact;
                        report.InventoryCashMoney = inventory.CashMoneyFact ?? 0;
                    }

                    if (message.TypeDoc == MoneyReportMessageTypeDoc.Arrival)
                        report.ArrivalsSum += message.Sum ?? 0;

                    if (message.TypeDoc == MoneyReportMessageTypeDoc.CashIncome)
                        report.CashIncome += message.Sum ?? 0;

                    if (message.TypeDoc == MoneyReportMessageTypeDoc.CashOutcome)
                        report.CashOutcome += message.Sum ?? 0;

                    if (message.TypeDoc == MoneyReportMessageTypeDoc.CheckMoney)
                        report.CashMoney += message.Sum ?? 0;

                    if (message.TypeDoc == MoneyReportMessageTypeDoc.CheckElectron)
                        report.CashElectron += message.Sum ?? 0;

                    if (message.TypeDoc == MoneyReportMessageTypeDoc.WriteOf)
                        report.Writeof += message.Sum ?? 0;

                    if (message.TypeDoc == MoneyReportMessageTypeDoc.RevaluationOld)
                        report.RevaluationOld += message.Sum ?? 0;

                    if (message.TypeDoc == MoneyReportMessageTypeDoc.RevaluationNew)
                        report.RevaluationNew += message.Sum ?? 0;

                    if (message.TypeDoc == MoneyReportMessageTypeDoc.CloseShift)
                        await calcItogWhereCloseShift(context, report, message);

                    addMessage(context, message);

                    await context.SaveChangesAsync();
                }
                catch(Exception ex)
                {
                    _logger.LogError("MoneyReportHostedService ошибка " + ex.Message+"\n message: "+message.ToString());
                    addMessage(context, message, ex.Message);
                    await context.SaveChangesAsync();
                }
            }
        }

        private void addMessage(OnlineShopContext context, MoneyReportMessageModel message, string? error = null)
        {
            var messageDb = _mapper.Map<MoneyReportMessage>(message);
            messageDb.Error = error;
            context.Add(messageDb);
        }

        /// <summary>
        /// Расчет итогов после закрытия смены, если она закрыта ночью на след. день
        /// </summary>
        /// <param name="context"></param>
        /// <param name="report"></param>
        /// <param name="shiftId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private async Task calcItogWhereCloseShift(OnlineShopContext context, MoneyReport report,  MoneyReportMessageModel message)
        {
            var shift = await context.Shifts.Where(x => x.Id == message.DocId).AsNoTracking().FirstOrDefaultAsync();
            if (shift == null)
                throw new Exception("Смена не найдена");
            
            if (shift.Start.Day == shift.Stop?.Day)
                return;
            //Если смена закрыта ночью
            report.StopGoodSum = await context.GoodCurrentBalances.Include(x => x.Good).ThenInclude(x => x.GoodPrices.Where(x => x.ShopId == message.ShopId))
                            .Where(x => x.ShopId == shift.ShopId)
                            .SumAsync(x => x.CurrentCount * x.Good.GoodPrices.First().Price);
            report.MoneyItog = report.StartCashMoney + report.CashMoney + report.InventoryCashMoney - report.CashOutcome;
            DateTime withoutTime = DateOnly.FromDateTime(DateTime.Now).ToDateTime(TimeOnly.MinValue);

            var nexReport = await context.MoneyReports.Where(x => DateTime.Compare(x.Create, withoutTime) == 0).FirstOrDefaultAsync();
            if (nexReport == null)
                throw new Exception("Новай отчет не найден");
            nexReport.StartGoodSum = report.StopGoodSum;
            nexReport.StartCashMoney = report.MoneyItog;

        }
    }
}
