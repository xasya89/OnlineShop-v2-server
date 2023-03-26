using AutoMapper;
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

        public async Task<GoodResponseModel> Get(int shopId, int id)
        {
            var response = await _context.Goods.Include(g=>g.GoodPrices.Where(p=>p.ShopId==shopId)).FirstAsync(g=>g.ShopId==shopId & g.Id==id);
            response.Price = response.GoodPrices.First().Price;
            return MapperConfigurationExtension.GetMapper().Map<GoodResponseModel>(response);
        }

        public async Task<GoodResponseModel> GetGoodByBarcode (int shopId, string barcodeStr)
        {
            var barcode = await _context.Barcodes
                .Include(g=>g.Good)
                .ThenInclude(g=>g.GoodPrices)
                .Where(b => b.Code == barcodeStr & !b.Good.IsDeleted).FirstOrDefaultAsync();
            if (barcode == null)
                throw new MyServiceException("Штрих код не найден");
            var good = barcode.Good;
            if(good.ShopId!=shopId)
                throw new MyServiceException("Товар не найден в текущем магазине");
            good.Price=good.GoodPrices.Where(p=>p.ShopId==shopId).FirstOrDefault()?.Price ?? good.Price;
            return MapperConfigurationExtension.GetMapper().Map<GoodResponseModel>(good);
        }

        public IEnumerable<GoodResponseModel> Search(string findText)
        {
            System.Diagnostics.Debug.WriteLine("findText - " + findText);
            return MapperConfigurationExtension.GetMapper().Map<IEnumerable<GoodResponseModel>>(
                _context.Goods.Where(g => !g.IsDeleted & EF.Functions.Like(g.Name.ToLower(), $"%{findText.ToLower()}%")).Take(20)
                );
        }
    }
}
