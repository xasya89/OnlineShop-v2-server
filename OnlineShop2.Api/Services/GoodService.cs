using Microsoft.EntityFrameworkCore;
using OnlineShop2.Api.Extensions;
using OnlineShop2.Api.Models.Goods;
using OnlineShop2.Database;

namespace OnlineShop2.Api.Services
{
    public class GoodService
    {
        private readonly OnlineShopContext _context;
        public GoodService(OnlineShopContext context) => _context = context;

        public async Task<GoodResponseModel> GetGoodByBarcode (int shopId, string barcodeStr)
        {
            var barcode = await _context.Barcodes
                .Include(g=>g.Good)
                .ThenInclude(g=>g.GoodPrices)
                .Where(b => b.Code == barcodeStr).FirstOrDefaultAsync();
            if (barcode == null)
                throw new MyServiceException("Штрих код не найден");
            var good = barcode.Good;
            if(good.ShopId!=shopId)
                throw new MyServiceException("Товар не найден в текущем магазине");
            good.Price=good.GoodPrices.Where(p=>p.ShopId==shopId).FirstOrDefault()?.Price ?? good.Price;
            return MapperConfigurationExtension.GetMapper().Map<GoodResponseModel>(good);
        }
    }
}
