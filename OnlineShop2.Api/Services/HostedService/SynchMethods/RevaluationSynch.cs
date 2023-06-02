using Microsoft.EntityFrameworkCore;
using OnlineShop2.Database;
using OnlineShop2.Database.Models;
using OnlineShop2.LegacyDb.Models;
using OnlineShop2.LegacyDb.Repositories;
using System.Diagnostics.CodeAnalysis;

namespace OnlineShop2.Api.Services.HostedService.SynchMethods
{
    public class RevaluationSynch
    {
        public static async Task Synch(OnlineShopContext context, [NotNull]IRevaluationRepositoryLegacy repositoryLegacy, int shopId)
        {
            var curDate = DateOnly.FromDateTime(DateTime.Now).ToDateTime(TimeOnly.MinValue);
            var tran = await context.Database.BeginTransactionAsync();
            try
            {
                var revaluationsLegacy = await repositoryLegacy.GetWithDate(curDate);
                if (revaluationsLegacy.Count() == 0)
                    return;
                var revaluations = await context.Revaluations.Include(r=>r.RevaluationGoods).Where(r => r.Create >= curDate).AsNoTracking().ToListAsync();

                var addList = await AnalyzeNewEntities(context, revaluationsLegacy, revaluations, shopId);
                if (addList.Count() == 0)
                    return;
                context.Revaluations.AddRange(addList);

                await context.SaveChangesAsync();
                await tran.CommitAsync();

                foreach (var item in addList)
                    context.Entry(item).State = EntityState.Detached;
            }
            catch(Exception e)
            {
                await tran.RollbackAsync();
            }
        }

        /// <summary>
        /// Получение новых записей из legacy db
        /// </summary>
        /// <param name="context"></param>
        /// <param name="legacyList"></param>
        /// <param name="dbList"></param>
        /// <returns></returns>
        private static async Task<IEnumerable<Revaluation>> AnalyzeNewEntities(OnlineShopContext context, IEnumerable<RevaluationLegacy> legacyList, IEnumerable<Revaluation> dbList, int shopId)
        {
            var legacyInDbIds = dbList.Select(x => x.LegacyId ?? 0);

            var legacyGoodsIds = dbList.SelectMany(x=>x.RevaluationGoods).GroupBy(x=>x.GoodId).Select(x=>x.Key);
            var goods = await context.Goods.Where(g => legacyGoodsIds.Contains(g.LegacyId ?? 0)).AsNoTracking().ToListAsync();

            return legacyList.Where(w => !legacyInDbIds.Contains(w.Id)).Select(legacy =>
            new Revaluation
            {
                ShopId = shopId,
                Create = legacy.Create,
                SumOld = legacy.SumOld,
                SumNew = legacy.SumNew,
                LegacyId = legacy.Id,
                RevaluationGoods = legacy.RevaluationGoods.Select(x => new RevaluationGood
                {
                    GoodId = goods.Find(g => g.LegacyId == x.GoodId).Id,
                    Count = x.Count,
                    PriceOld = x.PriceOld,
                    PriceNew = x.PriceNew
                })
            });
        }
    }
}
