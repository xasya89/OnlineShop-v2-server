using Microsoft.EntityFrameworkCore;
using OnlineShop2.Database.Models;
using OnlineShop2.Database;
using OnlineShop2.LegacyDb.Repositories;

namespace OnlineShop2.Api.Services.HostedService.SynchMethods
{
    internal class ArrivalSynch
    {
        public static async Task StartSynch(OnlineShopContext context, IUnitOfWorkLegacy unitOfWork, int shopId, MoneyReportChannelService moneyReportChannelService)
        {
            DateTime with = DateOnly.FromDateTime(DateTime.Now).ToDateTime(TimeOnly.MinValue);
            var laegacyArrivals = await unitOfWork.ArrivalRepository.GetArrivalWithDate(with);

            var arrivals = await context.Arrivals.Where(a => a.DateArrival >= with).ToListAsync();
            var arrivalLegacyIds = arrivals.Select(a => a.LegacyId);

            var suppliers = await context.Suppliers.AsNoTracking().ToListAsync();
            var goods = await context.Goods.AsNoTracking().ToListAsync();
            var newArrivals = laegacyArrivals.Where(a => !arrivalLegacyIds.Contains(a.Id)).Select(a =>
                new Arrival
                {
                    Id = 0,
                    Num = a.Num,
                    DateArrival = a.DateArrival,
                    SupplierId = suppliers.Find(s => s.LegacyId == a.SupplierId).Id,
                    ShopId = shopId,
                    Status = DocumentStatus.Successed,
                    PurchaseAmount = a.SumArrival,
                    SaleAmount = a.SumSell,
                    SumNds = a.SumNds,
                    LegacyId = a.Id,
                    ArrivalGoods = a.ArrivalGoods.Select(x => new ArrivalGood
                    {
                        Id = 0,
                        ArrivalId = 0,
                        GoodId = goods.Find(g => g.LegacyId == x.GoodId).Id,
                        Count = x.Count,
                        PricePurchase = x.Price,
                        PriceSell = x.PriceSell,
                        Nds = NDSType.None,
                        ExpiresDate = x.ExpiresDate
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

            foreach (var arrival in newArrivals)
                moneyReportChannelService.PushArrival(arrival.Id, arrival.DateArrival, shopId, arrival.SaleAmount);

            foreach (var newArrival in newArrivals)
                context.Entry(newArrival).State = EntityState.Detached;
        }
    }
}
