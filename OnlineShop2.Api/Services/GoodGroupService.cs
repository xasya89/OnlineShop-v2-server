using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using OnlineShop2.Api.Extensions;
using OnlineShop2.Api.Models.Goods;
using OnlineShop2.Api.Services.Legacy;
using OnlineShop2.Database;
using OnlineShop2.Database.Models;
using OnlineShop2.LegacyDb.Models;
using OnlineShop2.LegacyDb.Repositories;

namespace OnlineShop2.Api.Services
{
    public class GoodGroupService
    {
        private bool ownerGoodForShops;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly OnlineShopContext _context;
        private IUnitOfWorkLegacy _unitOfWorkLegacy;

        public GoodGroupService(IConfiguration configuration, IMapper mapper, OnlineShopContext context, IUnitOfWorkLegacy unitOfWorkLegacy)
        {
            _configuration = configuration;
            _context = context;
            _mapper = mapper;
            _unitOfWorkLegacy = unitOfWorkLegacy;
            ownerGoodForShops = configuration.GetValue<bool>("OwnerGoodForShops");
        }

        public async Task<IEnumerable<GoodGroupCreateRequestModel>> GetAll(int shopId) =>
            _mapper.Map<IEnumerable<GoodGroupCreateRequestModel>>(
                await _context.GoodsGroups.Where(g => !ownerGoodForShops || g.ShopId == shopId).OrderBy(g => g.Name).ToListAsync()
                );

        public async Task<GoodGroupCreateRequestModel> Create(int shopId, GoodGroupCreateRequestModel model)
        {
            var group = _mapper.Map<GoodGroup>(model);
            group.ShopId = shopId;
            var entityEntry = _context.Add(group);
            await SaveChangeLegacy(entityEntry);
            _context.SaveChanges();
            return _mapper.Map<GoodGroupCreateRequestModel>(group);
        }

        public async Task<GoodGroupCreateRequestModel> Update(GoodGroupCreateRequestModel model)
        {
            var group = await _context.GoodsGroups.FindAsync(model.Id);
            if (group == null) throw new MyServiceException($"Группа с id {model.Id} не найдена");
            _context.ChangeEntityByDTO<GoodGroupCreateRequestModel>(_context.Entry(group), model);
            await SaveChangeLegacy(_context.Entry(group));
            await _context.SaveChangesAsync();
            return _mapper.Map<GoodGroupCreateRequestModel>(group);
        }

        public async Task Delete(int id)
        {
            var group = await _context.GoodsGroups.FindAsync(id);
            if (group == null) throw new MyServiceException($"Группа с id {id} не найдена");
            var entityEntry = _context.Remove(group);
            await SaveChangeLegacy(entityEntry);
            await _context.SaveChangesAsync();
        }

        private async Task SaveChangeLegacy(EntityEntry entity )
        {

            var goodGroup = entity.Entity as GoodGroup;
            var shop = _context.Shops.AsNoTracking().First(x => x.Id == goodGroup.ShopId);
            if (shop.LegacyDbNum == null)
                return;
            _unitOfWorkLegacy.SetConnectionString(_configuration.GetConnectionString("shop" + shop.LegacyDbNum));
            if (entity.State == EntityState.Added)
                goodGroup.LegacyId = await _unitOfWorkLegacy.GoodGroupRepository.AddAsync(new GoodGroupLegacy
                {
                    Name = goodGroup.Name
                });
            if (entity.State == EntityState.Modified & goodGroup.LegacyId!=null)
                await _unitOfWorkLegacy.GoodGroupRepository.UpdateAsync(new GoodGroupLegacy
                {
                    Name = goodGroup.Name,
                    Id = goodGroup.LegacyId ?? 0
                });
            if (entity.State == EntityState.Deleted & goodGroup.LegacyId != null)
                await _unitOfWorkLegacy.GoodGroupRepository.DeleteAsync(goodGroup.LegacyId??0);
        }
    }
}
