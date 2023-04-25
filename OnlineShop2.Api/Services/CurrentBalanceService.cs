using Microsoft.EntityFrameworkCore;
using OnlineShop2.Api.Models.CurrentBalance;
using OnlineShop2.Database;
using OnlineShop2.Database.Models;

namespace OnlineShop2.Api.Services
{
    public class CurrentBalanceService
    {
        private readonly OnlineShopContext _context;
        public CurrentBalanceService(OnlineShopContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CurrentBalanceResponseModel>> GetBalance (int shopId, bool skipDeleted, bool viewNegativeBalance, int[] groups, int[] suppliers)
        {
            var query = _context.GoodCurrentBalances
                .Include(b => b.Good)
                .ThenInclude(b => b.GoodPrices.Where(p => p.ShopId == shopId))
                .Where(b=>b.ShopId==shopId);

            if (skipDeleted)
                query = query.Where(g => !g.Good.IsDeleted);
            if (groups?.Length > 0)
                query = query.Where(g => groups.Contains(g.Good.GoodGroupId));
            if (suppliers?.Length > 0)
                query = query.Where(g => suppliers.Contains(g.Good.SupplierId ?? 0));
            if (!viewNegativeBalance)
                query = query.Where(g => g.CurrentCount > 0);
            return await query.AsNoTracking().Select(b=>new CurrentBalanceResponseModel
            {
                Id=b.Id,
                GoodId=b.Good.Id,
                Name=b.Good.Name,
                GoodGroupId=b.Good.GoodGroupId,
                SupplierId=b.Good.SupplierId,
                Price=b.Good.Price,
                ShopId=shopId,
                Unit=b.Good.Unit,
                CurrentCount=b.CurrentCount
            }).ToListAsync();
        }
            
    }
}
