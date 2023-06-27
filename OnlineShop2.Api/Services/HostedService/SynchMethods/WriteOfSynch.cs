using OnlineShop2.Database.Models;
using OnlineShop2.Database;
using OnlineShop2.LegacyDb.Repositories;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace OnlineShop2.Api.Services.HostedService.SynchMethods
{
    public class WriteOfSynch
    {
        public static async Task StartSync(OnlineShopContext context, IMapper mapper, IUnitOfWorkLegacy unitOfWork, int shopId, MoneyReportChannelService moneyReportChannelService)
        {
            DateTime with = DateOnly.FromDateTime(DateTime.Now).ToDateTime(TimeOnly.MinValue);
            var legacyWriteofs = await unitOfWork.WriteofRepositoryLegacy.GetWriteOfWithDate(DateTime.Now);
            if (legacyWriteofs == null)
                return;
            var legacyList = mapper.Map<IEnumerable<Writeof>>(legacyWriteofs);
            var legacyInDbIds = await context.Writeofs.Where(w => w.DateWriteof >= with).AsNoTracking().Select(w => w.LegacyId).ToListAsync();

            var legacyGoodsIds = legacyList.SelectMany(w => w.WriteofGoods).GroupBy(x => x.GoodId).Select(x => x.Key).ToList();
            var goods = await context.Goods.Where(g => legacyGoodsIds.Contains(g.LegacyId ?? 0)).AsNoTracking().ToListAsync();

            var newWriteofs = new List<Writeof>();
            foreach (var legacy in legacyList.Where(w => !legacyInDbIds.Contains(w.Id)))
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
                newWriteofs.Add(legacy);
            }
            context.Writeofs.AddRange(newWriteofs);

            await context.SaveChangesAsync();

            foreach (var writeof in newWriteofs)
                moneyReportChannelService.PushWriteOf(writeof.Id, writeof.DateWriteof, shopId, writeof.SumAll);
        }
    }
}
