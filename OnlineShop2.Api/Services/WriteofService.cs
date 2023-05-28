using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using OnlineShop2.Api.BizLogic;
using OnlineShop2.Api.Extensions;
using OnlineShop2.Api.Models.Writeof;
using OnlineShop2.Database;
using OnlineShop2.Database.Models;
using OnlineShop2.LegacyDb.Models;
using OnlineShop2.LegacyDb.Repositories;

namespace OnlineShop2.Api.Services
{
    public class WriteofService
    {
        private readonly ILogger<WriteofService> _logger;
        private readonly IConfiguration _configuration;
        private readonly OnlineShopContext _context;
        private readonly IMapper _mapper;
        private readonly IUnitOfWorkLegacy _unitOfWorkLegacy;

        public WriteofService(ILogger<WriteofService> logger, IConfiguration configuration, OnlineShopContext context, IMapper mapper, IUnitOfWorkLegacy unitOfWorkLegacy)
        {
            _logger = logger;
            _configuration = configuration;
            _context = context;
            _mapper = mapper;
            _unitOfWorkLegacy = unitOfWorkLegacy;
        }

        public async Task<dynamic> GetAll(int shopId, int page=0, int count=20)
        {
            var total = await _context.Writeofs.CountAsync();
            var result = _mapper.Map<IEnumerable<WriteofSummaryModel>>(
                await _context.Writeofs.Where(w => w.ShopId == shopId).Skip((page - 1) * count).Take(count).ToListAsync()
            );
            return new { Total = total, writeofs = result };
        }

        public async Task<WriteofModel> GetOne(int writeofId) =>
            _mapper.Map<WriteofModel>(
                await _context.Writeofs.Include(w => w.WriteofGoods).ThenInclude(w=>w.Good).Where(w => w.Id == writeofId).FirstAsync());

        public async Task<WriteofModel> Add(WriteofModel model)
        {
            var writeof = _mapper.Map<Writeof>(model);
            var entity = _context.Writeofs.Add(writeof);
            var balanceChange = writeof.WriteofGoods.GroupBy(w => w.GoodId)
                .Select(w => new { GoodId = w.Key, Count = -1 * w.Sum(x => x.Count) })
                .ToDictionary(x => x.GoodId, x => x.Count);
            await CurrentBalanceChange.Change(_context, model.ShopId, balanceChange);

            await modifiLegacy(entity);

            await _context.SaveChangesAsync();
            return _mapper.Map<WriteofModel>(
                await _context.Writeofs.Include(w => w.WriteofGoods).ThenInclude(w=>w.Good).Where(w => w.Id == writeof.Id).AsNoTracking().FirstAsync()
                );
        }

        public async Task<WriteofModel> Update(WriteofModel model)
        {
            var writeof = _mapper.Map<Writeof>(model);
            writeof.SumAll = writeof.WriteofGoods.Sum(w => w.Count * w.Price);
            var entity = _context.Writeofs.Update(writeof);

            var positionsOriginal = await _context.WriteofGoods.Where(w => w.WriteofId == writeof.Id).AsNoTracking().ToListAsync();

            await CurrentBalanceChange.Change(_context, writeof.ShopId, 
                positionsOriginal.GroupBy(x => x.GoodId).ToDictionary(x => x.Key, x => x.Sum(x => x.Count)));
            await CurrentBalanceChange.Change(_context, writeof.ShopId,
                writeof.WriteofGoods.GroupBy(w => w.GoodId).ToDictionary(x => x.Key, x => -1 * x.Sum(x => x.Count)));

            var positionsOriginalIds = positionsOriginal.Select(x=>x.Id);
            var positionsIds = writeof.WriteofGoods.Select(w => w.Id);
            var deletePositions = positionsOriginalIds.Where(id=>!positionsIds.Contains(id));
            await _context.WriteofGoods.Where(x => deletePositions.Contains(x.Id)).ExecuteDeleteAsync();

            await modifiLegacy(entity);

            await _context.SaveChangesAsync();

            return _mapper.Map<WriteofModel>(writeof);
        }

        public async Task Delete(int writeofId)
        {
            var original = await _context.Writeofs.Include(x=>x.WriteofGoods)
                .Where(w => w.Id == writeofId).AsNoTracking().FirstOrDefaultAsync();
            if (original == null)
                throw new MyServiceException($"Списание с id {writeofId} не найден");

            await CurrentBalanceChange.Change(_context, original.ShopId,
                original.WriteofGoods.GroupBy(x => x.GoodId).ToDictionary(x => x.Key, x => x.Sum(x => x.Count)));

            var writeof = await _context.Writeofs.Where(w => w.Id == writeofId).FirstAsync();
            var entity = _context.Remove(writeof);

            modifiLegacy(entity);

            await _context.SaveChangesAsync();
        }

        private async Task modifiLegacy(EntityEntry entity)
        {
            var writeof = entity.Entity as Writeof;
            var writeofLegacy = _mapper.Map<WriteofLegacy>(writeof);
            var shop = await _context.Shops.AsNoTracking().FirstAsync(x => x.Id == writeof.ShopId);
            if (shop.LegacyDbNum == null)
                return;
            var connectionStr = _configuration.GetConnectionString("shop" + shop.LegacyDbNum);
            _unitOfWorkLegacy.SetConnectionString(connectionStr);
            var writeofRepositoryLegacy = _unitOfWorkLegacy.WriteofRepositoryLegacy;
            writeofLegacy.Id = writeof.LegacyId ?? 0;
            if (entity.State == EntityState.Deleted)
            {
                await writeofRepositoryLegacy.DeleteAsync(writeofLegacy.Id);
                return;
            }

            var goodsIds = writeof.WriteofGoods.Select(w => w.GoodId);
            var goods = await _context.Goods.Where(g => goodsIds.Contains(g.Id)).AsNoTracking().ToListAsync();
            writeofLegacy.WriteofGoods.ForEach(w => w.GoodId = goods.Find(x=>x.Id == w.GoodId).LegacyId ?? 0);

            if (entity.State == EntityState.Added)
                await writeofRepositoryLegacy.AddAsync(writeofLegacy);
            if (entity.State == EntityState.Modified)
                await writeofRepositoryLegacy.UpdateAsync(writeofLegacy);

        }
    }
}
