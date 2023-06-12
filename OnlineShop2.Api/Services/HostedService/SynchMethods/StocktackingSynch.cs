using Microsoft.EntityFrameworkCore;
using OnlineShop2.Database;
using OnlineShop2.LegacyDb.Repositories;

namespace OnlineShop2.Api.Services.HostedService.SynchMethods
{
    public class StocktackingSynch
    {
        public static async Task StartSynch(OnlineShopContext context, IUnitOfWorkLegacy unitOfWork, int shopId, MoneyReportChannelService moneyReportChannelService)
        {
            var repository = unitOfWork.StocktackingRepositoryLegacy;
            var newRepositories = await repository.GetNewStocktacking();
            if (newRepositories == null)
                return;
            var currentBalanceLegacy = await unitOfWork.CurrentBalance.GetCurrent();
            var currentBalance = await context.GoodCurrentBalances.Include(g=>g.Good)
                .Where(x => x.ShopId == shopId & !x.Good.IsDeleted).AsNoTracking().ToListAsync();
            foreach(var item in currentBalance)
            {
                var itemLegacy = currentBalanceLegacy.Where(x => x.GoodId == item.Good.LegacyId).FirstOrDefault();
                var count = itemLegacy?.Count ?? 0;
                await context.GoodCurrentBalances.Where(x => x.Id == item.Id)
                    .ExecuteUpdateAsync(x => x.SetProperty(x => x.CurrentCount, count));
            }
            var create = DateOnly.FromDateTime(newRepositories.Create).ToDateTime(TimeOnly.MinValue);
            moneyReportChannelService.PushInventoryCashSumLegacy(create, shopId, newRepositories.CashMoneyFact);
            moneyReportChannelService.PushInventoryGoodSumLegacy(create, shopId, newRepositories.SumFact);
            await repository.SetCompliteProccessing();
        }
    }
}
