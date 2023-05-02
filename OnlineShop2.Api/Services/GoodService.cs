using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using OnlineShop2.Api.Extensions;
using OnlineShop2.Api.Models.Goods;
using OnlineShop2.Database;
using OnlineShop2.Database.Models;
using OnlineShop2.LegacyDb.Models;
using OnlineShop2.LegacyDb.Repositories;
using System.Collections;
using System.Collections.Specialized;
using System.Linq;

namespace OnlineShop2.Api.Services
{
    public class GoodService
    {
        private readonly IConfiguration _configuration;
        private readonly OnlineShopContext _context;
        private readonly IMapper _mapper;
        private readonly IUnitOfWorkLegacy _unitOfWorkLegacy;
        private bool ownerGoodForShops = false;
        public GoodService(OnlineShopContext context, IConfiguration configuration, IMapper mapper, IUnitOfWorkLegacy unitOfWorkLegacy)
        {
            _configuration = configuration;
            ownerGoodForShops = _configuration.GetValue<bool>("OwnerGoodForShops");
            _context = context;
            _mapper = mapper;
            _unitOfWorkLegacy = unitOfWorkLegacy;
        }

        public async Task<dynamic> GetAll(int shopId, int[] groups, bool skipDeleted, string? find, int page=1, int count=100)
        {
            var query = _context.Goods
                .Include(g => g.Barcodes).Include(g => g.GoodPrices).Include(g => g.GoodGroup).Include(g => g.Supplier)
                .Where(g=> EF.Functions.ILike(g.Name, $"%{find}%") || g.Barcodes.Where(b => b.Code == find).Any());
            
            if (ownerGoodForShops)
                query = query.Where(g => g.ShopId == shopId);
            if (groups != null && groups.Length>0)
                query = query.Where(g => groups.Contains(g.GoodGroupId));
            if (skipDeleted)
                query = query.Where(g => !g.IsDeleted);


            int total = await query.CountAsync();
            var goods = await query.Skip((page - 1) * count).Take(count).ToListAsync();
            goods.ForEach(g => g.Price = g.GoodPrices.Where(p => p.ShopId == shopId).FirstOrDefault()?.Price ?? 0);
            return new
            {
                Total = total,
                Goods = _mapper.Map<IEnumerable<GoodResponseModel>>(goods)
            };
        }

        public async Task<GoodResponseModel> GetOne(int shopId, int id)
        {
            var query = _context.Goods.Include(g => g.Barcodes).Include(g => g.Price).Include(g => g.Supplier).Include(g => g.GoodGroup)
                .Where(g => g.Id == id);
            var good = await query.FirstOrDefaultAsync();
            if (good == null)
                throw new MyServiceException($"Товар id {id} не найден");
            return _mapper.Map<GoodResponseModel>(good);
        }

        public async Task<GoodResponseModel> Create(int shopId, GoodCreateRequestModel model)
        {
            if (string.IsNullOrEmpty(model.Name)) throw new MyServiceException("Не указано наименование товара");
            var barcodes = model.Barcodes.Select(b=>b.Code);
            var barcodeInDb =await _context.Barcodes.Include(b=>b.Good).Where(b => barcodes.Contains(b.Code)).ToArrayAsync();
            if (!ownerGoodForShops && barcodeInDb.Count()>0)
                throw new MyServiceException($"Товар с штрих кодом {string.Join(" ", barcodeInDb.Select(b=>b.Code))} существует");
            if(ownerGoodForShops && barcodeInDb.Where(b=>b.Good.ShopId==shopId).Any())
                throw new MyServiceException($"Товар с штрих кодом {string.Join(" ", barcodeInDb.Where(b => b.Good.ShopId == shopId).Select(b => b.Code))} существует");
            var good = _mapper.Map<Good>(model);
            good.ShopId = shopId;
            var entity = _context.Add(good);
            _context.GoodCurrentBalances.AddRange(good.GoodPrices.Select(p => new GoodCurrentBalance { Good = good, ShopId = p.ShopId, CurrentCount = 0 }));
            await saveChangedLegacy(entity);
            await _context.SaveChangesAsync();
            return _mapper.Map<GoodResponseModel>(good);
        }

        public async Task<GoodResponseModel> Update(int shopId, GoodCreateRequestModel model)
        {
            var good = await _context.Goods.Include(g=>g.GoodPrices).Include(g=>g.Barcodes).Where(g=>g.Id==model.Id).FirstOrDefaultAsync();
            if (good == null) throw new MyServiceException($"Товар с id {model.Id} не найден");

            _context.Entry(good).State = EntityState.Modified;
            _context.ChangeEntityByDTO<GoodCreateRequestModel>(_context.Entry(good), model);
            good.GoodPrices.ForEach(price => _context.ChangeEntityByDTO<GoodPriceCreateRequestModel>(_context.Entry(price), model.GoodPrices.Where(p => p.Id == price.Id).First()));
            good.Barcodes.ForEach(barcode => _context.ChangeEntityByDTO<BarcodeCreateRequestModel>(_context.Entry(barcode), model.Barcodes.Where(p => p.Id == barcode.Id).First()));
            _context.GoodPrices.AddRange(model.GoodPrices.Where(p => p.Id == 0).Select(p => new GoodPrice { GoodId=model.Id,  ShopId = p.ShopId, Price = p.Price }) );
            var newBarcodes = model.Barcodes.Where(b => b.Id == 0 & !b.IsDeleted).Select(b => new Barcode { Good = good, Code = b.Code });
            var countExistCode = await _context.Barcodes.Where(b => newBarcodes.Select(x => x.Code).Contains(b.Code)).CountAsync();
            if (countExistCode > 0)
                throw new MyServiceException("Штрих код уже существует");
            _context.Barcodes.AddRange(newBarcodes);

            var deletedBarcodesId = model.Barcodes.Where(b => b.Id!=0 & b.IsDeleted).Select(b => b.Id);
            _context.Barcodes.RemoveRange(good.Barcodes.Where(b => deletedBarcodesId.Contains(b.Id)));
            await saveChangedLegacy(_context.Entry(good));
            await _context.SaveChangesAsync();
            return _mapper.Map<GoodResponseModel>(good);
        }

        public async Task Delete(int id)
        {
            var good = await _context.Goods.Include(g=>g.GoodPrices).Include(g=>g.Barcodes).Where(g=>g.Id==id).FirstOrDefaultAsync();
            if (good == null) throw new MyServiceException($"Товар с id {id} не найден");

            //Проверим существование товара в дургих документах
            bool flag = true;
            flag = await _context.CheckGoods.Where(c=>c.GoodId==id).AnyAsync() ? false : flag;
            flag = await _context.InventoryGoods.Where(c => c.GoodId == id).AnyAsync() ? false : flag;
            if (flag)
                _context.Remove(good);
            else
                good.IsDeleted = true;
            await saveChangedLegacy(_context.Entry(good));
            await _context.SaveChangesAsync();
        }

        public async Task<GoodResponseModel> Get(int shopId, int id)
        {
            var response = await _context.Goods
                .Include(g=>g.Barcodes)
                .Include(g=>g.GoodPrices.Where(p=>p.ShopId==shopId))
                .FirstAsync(g=>g.ShopId==shopId & g.Id==id);
            response.Price = response.GoodPrices.First().Price;
            return _mapper.Map<GoodResponseModel>(response);
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
            return _mapper.Map<GoodResponseModel>(good);
        }

        public IEnumerable<GoodResponseModel> Search(string findText)
        {
            System.Diagnostics.Debug.WriteLine("findText - " + findText);
            return _mapper.Map<IEnumerable<GoodResponseModel>>(
                _context.Goods.Where(g => !g.IsDeleted & EF.Functions.Like(g.Name.ToLower(), $"%{findText.ToLower()}%")).Take(20)
                );
        }

        private async Task saveChangedLegacy(EntityEntry entity)
        {
            var good = entity.Entity as Good;
            var shop = _context.Shops.Find(good.ShopId);
            if (shop.LegacyDbNum == null)
                return;
            _unitOfWorkLegacy.SetConnectionString(_configuration.GetConnectionString("shop" + shop.LegacyDbNum));

            if (entity.State == EntityState.Deleted & good.LegacyId != null)
            {
                await _unitOfWorkLegacy.GoodRepository.DeleteAsync(good.LegacyId ?? 0);
                return;
            }

            var groups = await _context.GoodsGroups.Where(gr => gr.ShopId == shop.Id).AsNoTracking().ToListAsync();
            var supplers = await _context.Suppliers.Where(s => s.ShopId == shop.Id).AsNoTracking().ToListAsync();
            var goodLegacy = _mapper.Map<GoodLegacy>(good);
            goodLegacy.Id = good.LegacyId ?? 0;
            goodLegacy.GoodGroupId = groups.Find(g => g.Id == good.GoodGroupId).LegacyId ?? 0;
            if (goodLegacy.SupplierId != null)
                goodLegacy.SupplierId = supplers.Find(s => s.Id == goodLegacy.SupplierId).LegacyId ?? 0;
            goodLegacy.Barcodes = _mapper.Map<List<BarCodeLegacy>>(good.Barcodes.Where(b => _context.Entry(b).State != EntityState.Deleted).ToList());

            if (entity.State == EntityState.Added)
                good.LegacyId = await _unitOfWorkLegacy.GoodRepository.AddAsync(goodLegacy);
            if (entity.State == EntityState.Modified & good.LegacyId != null)
                await _unitOfWorkLegacy.GoodRepository.UpdateAsync(goodLegacy);
        }

        //TODO: Перенести метод сравнения объектов
        private void compareAndChange(object original, object dist, bool isNotChangeId = true)
        {
            var originalType = original.GetType();
            var distProps = dist.GetType().GetProperties();
            var simpleTypes = new Type[] { typeof(int), typeof(string), typeof(decimal), typeof(bool), typeof(Guid) };
            string[] collectionsName = new[] { nameof(IList), nameof(ICollection), nameof(IEnumerable) };

            var propsOriginal = original.GetType().GetProperties();
            if (isNotChangeId)
                propsOriginal = propsOriginal.Where(p => p.Name.ToLower() != "id").ToArray();
            var simpleProps = propsOriginal.Where(p => p.PropertyType.GetInterfaces().Count(x => collectionsName.Contains(x.Name)) == 0);
            foreach (var prop in simpleProps)
            {
                var oldValue = prop.GetValue(original);
                var distprop = distProps.Where(p => p.Name == prop.Name);
                distprop?.First().SetValue(dist, oldValue);
            }

            var collectionsProp = propsOriginal.Where(p => p.PropertyType != typeof(string) & p.PropertyType.GetInterfaces().Count(x => collectionsName.Contains(x.Name)) != 0);
            foreach (var prop in collectionsProp)
            {
                var items = prop.GetValue(original, null) as IEnumerable;
                foreach (var item in items)
                    //compareAndChange()
                    Console.WriteLine(string.Join(",", item.GetType().GetProperties().Select(x => x.Name)));
                Type type = prop.PropertyType.GetGenericArguments()[0];
                Type listType = typeof(List<>).MakeGenericType(type);

                var val = prop.GetValue(original);

            }
        }
    }
}
