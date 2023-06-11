using Google.Protobuf;
using OnlineShop2.Api.Models.ReportMessage;
using System.Threading.Channels;
using OnlineShop2.Dao;

namespace OnlineShop2.Api.Services
{
    public class MoneyReportChannelService
    {
        private readonly ILogger<MoneyReportChannelService> _logger;
        private readonly Channel<MoneyReportMessageModel> _channel;

        public MoneyReportChannelService(ILogger<MoneyReportChannelService> logger)
        {
            _logger = logger;
            _channel = Channel.CreateUnbounded<MoneyReportMessageModel>(new UnboundedChannelOptions
            {
                AllowSynchronousContinuations = true,
                SingleReader = false,
                SingleWriter = false
            });
        }

        public void Push(MoneyReportMessageModel message) => _channel.Writer.TryWrite(message);

        public void PushInventory(int id, int shopId) => _channel.Writer.TryWrite(
            new MoneyReportMessageModel(MoneyReportMessageTypeDoc.InventoryComplite, DateTime.Now, shopId, id));

        public void PushWriteOf(int id, DateTime date, int shopId, decimal sum) =>
            _channel.Writer.TryWrite(new MoneyReportMessageModel(
                MoneyReportMessageTypeDoc.WriteOf, date, shopId, id, sum
                ));

        public void PushArrival(int id, DateTime date, int shopId, decimal sum) =>
            _channel.Writer.TryWrite(new MoneyReportMessageModel(
                MoneyReportMessageTypeDoc.Arrival, date, shopId, id, sum
                ));

        public void PushOpenShift(DateTime date, int shopId, int shiftId) =>
            _channel.Writer.TryWrite(new MoneyReportMessageModel(
                MoneyReportMessageTypeDoc.OpenShift, date, shopId, shiftId
                ));

        public void PushCloseShift(DateTime date, int shopId, int shiftId) =>
            _channel.Writer.TryWrite(new MoneyReportMessageModel(
                MoneyReportMessageTypeDoc.CloseShift, date, shopId, shiftId
                ));

        public void PushCheckMoney(DateTime date, int shopId, int checkId, decimal sum) =>
            _channel.Writer.TryWrite(new MoneyReportMessageModel(
                MoneyReportMessageTypeDoc.CheckMoney, date, shopId, checkId, sum
                ));

        public void PushCheckElectron(DateTime date, int shopId, int checkId, decimal sum) =>
            _channel.Writer.TryWrite(new MoneyReportMessageModel(
                MoneyReportMessageTypeDoc.CheckElectron, date, shopId, checkId, sum
                ));

        public async Task<MoneyReportMessageModel> PullAsync(CancellationToken token) =>
            await _channel.Reader.ReadAsync(token);
    }
}
