using Microsoft.EntityFrameworkCore;
using OnlineShop2.Database;

namespace OnlineShop2.Api.BizLogic
{
    public static class PriceChange
    {
        public static async Task Change(OnlineShopContext _context, int shopId, Dictionary<int, decimal> goodprices)
        {
            var goodsid = goodprices.Select(p => p.Key);
            var priceoriginal = await _context.GoodPrices.Where(p => p.ShopId == shopId & goodsid.Contains(p.GoodId)).AsNoTracking().ToListAsync();
            var diff = from price in goodprices
                       join original in priceoriginal on price.Key equals original.GoodId
                       where price.Value != original.Price
                       select new { GoodId = price.Key, NewPrice = price.Value };
            foreach (var item in diff)
                await _context.GoodPrices.Where(p => p.GoodId == item.GoodId).ExecuteUpdateAsync(p => p.SetProperty(x => x.Price, item.NewPrice));
        }
    }
}
