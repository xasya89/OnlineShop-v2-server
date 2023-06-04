using Microsoft.EntityFrameworkCore;
using OnlineShop2.Database;

namespace OnlineShop2.Api.BizLogic
{
    public static class CurrentBalanceChange
    {
        public static async Task Change(OnlineShopContext _context, int shopId, Dictionary<int, decimal> balances)
        {
            foreach (var balance in balances)
                await _context.GoodCurrentBalances.Where(b => b.GoodId == balance.Key).ExecuteUpdateAsync(b =>
                b.SetProperty(p => p.CurrentCount, p => p.CurrentCount + balance.Value));
        }
    }
}
