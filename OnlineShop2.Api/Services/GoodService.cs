using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using OnlineShop2.Api.Extensions;
using OnlineShop2.Api.Models.Goods;
using OnlineShop2.Database;
using OnlineShop2.Database.Models;
using System.Collections;
using System.Collections.Specialized;

namespace OnlineShop2.Api.Services
{
    public class GoodService
    {
        private readonly IConfiguration _configuration;
        private readonly OnlineShopContext _context;
        private bool ownerGoodForShops = false;
        public GoodService(OnlineShopContext context, IConfiguration configuration)
        {
            _configuration = configuration;
            ownerGoodForShops = _configuration.GetValue<bool>("OwnerGoodForShops");
            _context = context;
        }

        public async Task<IEnumerable<Good>> GetAll(int shopId, int? groupId, bool skipDeleted, string? find, int page=1, int count=100)
        {
            var query = _context.Goods.Include(g => g.Barcodes).Include(g => g.Price).Include(g=>g.GoodGroup).Include(g=>g.Supplier)
                .Where(g => EF.Functions.ILike(g.Name, $"%{find}%") || g.Barcodes.Where(b => b.Code == find).Any());
            if (ownerGoodForShops)
                query = query.Where(g => g.ShopId == shopId);
            if (groupId != null)
                query = query.Where(g => g.GoodGroupId == groupId);
            if (skipDeleted)
                query = query.Where(g => !g.IsDeleted);
            return await query.Skip((page-1)*count).Take(count).ToArrayAsync();
        }

        public async Task<GoodResponseModel> GetOne(int shopId, int id)
        {
            var query = _context.Goods.Include(g => g.Barcodes).Include(g => g.Price).Include(g => g.Supplier).Include(g => g.GoodGroup)
                .Where(g => g.Id == id);
            var good = await query.FirstOrDefaultAsync();
            if (good == null)
                throw new MyServiceException($"Товар id {id} не найден");
            return MapperConfigurationExtension.GetMapper().Map<GoodResponseModel>(good);
        }

        public async Task<GoodResponseModel> Create(int shopId, GoodCreateRequestModel model)
        {
            var barcodes = model.Barcodes.Select(b=>b.Code);
            var barcodeInDb =await _context.Barcodes.Include(b=>b.Good).Where(b => barcodes.Contains(b.Code)).ToArrayAsync();
            if (!ownerGoodForShops && barcodeInDb.Count()>0)
                throw new MyServiceException($"Товар с штрих кодом {string.Join(" ", barcodeInDb.Select(b=>b.Code))} существует");
            if(ownerGoodForShops && barcodeInDb.Where(b=>b.Good.ShopId==shopId).Any())
                throw new MyServiceException($"Товар с штрих кодом {string.Join(" ", barcodeInDb.Where(b => b.Good.ShopId == shopId).Select(b => b.Code))} существует");
            var good = MapperConfigurationExtension.GetMapper().Map<Good>(model);
            good.ShopId = shopId;
            _context.Add(good);
            await _context.SaveChangesAsync();
            return MapperConfigurationExtension.GetMapper().Map<GoodResponseModel>(good);
        }

        public async Task<GoodResponseModel> Update(int shopId, GoodCreateRequestModel model)
        {
            var good = await _context.Goods.Include(g=>g.GoodPrices).Include(g=>g.Barcodes).Where(g=>g.Id==model.Id).FirstOrDefaultAsync();
            if (good == null) throw new MyServiceException($"Товар с id {model.Id} не найден");

            Compare<GoodCreateRequestModel>(_context.Entry(good), model);
            good.GoodPrices.ForEach(price => Compare<GoodPriceCreateRequestModel>(_context.Entry(price), model.GoodPrices.Where(p => p.Id == price.Id).First()));
            good.Barcodes.ForEach(barcode => Compare<BarcodeCreateRequestModel>(_context.Entry(barcode), model.Barcodes.Where(p => p.Id == barcode.Id).First()));

            await _context.SaveChangesAsync();
            return MapperConfigurationExtension.GetMapper().Map<GoodResponseModel>(good);
        }

        private void Compare<T>(EntityEntry entity, T model)
        {
            string[] collectionsName = new[] { nameof(IList), nameof(ICollection), nameof(IEnumerable) };
            var propertes = typeof(T).GetProperties().Where(p=>p.Name.ToLower()!="id");
            foreach (var name in collectionsName)
                propertes = propertes.Where(p => p.PropertyType==typeof(string) || p.PropertyType.GetInterface(name) == null);
            foreach(var prop in propertes)
            {
                var entittyProp = entity.Metadata.GetProperties().Where(p => p.Name == prop.Name).FirstOrDefault();
                if(entittyProp!=null)
                    entity.Property(entittyProp.Name).CurrentValue= prop.GetValue(model);
            }
        }

        public async Task<GoodResponseModel> Get(int shopId, int id)
        {
            var response = await _context.Goods
                .Include(g=>g.Barcodes)
                .Include(g=>g.GoodPrices.Where(p=>p.ShopId==shopId))
                .FirstAsync(g=>g.ShopId==shopId & g.Id==id);
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
