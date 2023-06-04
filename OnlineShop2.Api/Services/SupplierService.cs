using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using OnlineShop2.Api.Models.Goods;
using OnlineShop2.Api.Services.Legacy;
using OnlineShop2.Database;
using OnlineShop2.Database.Models;
using OnlineShop2.LegacyDb;
using OnlineShop2.LegacyDb.Models;
using OnlineShop2.LegacyDb.Repositories;

namespace OnlineShop2.Api.Services
{
    public class SupplierService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWorkLegacy _unitOfWorkLegacy;
        private readonly IConfiguration _configuration;
        private readonly OnlineShopContext _context;

        public SupplierService(OnlineShopContext context, IConfiguration configuration, IMapper mapper, IUnitOfWorkLegacy unitOfWorkLegacy)
        {
            _context = context;
            _configuration = configuration;
            _mapper = mapper;
            _unitOfWorkLegacy = unitOfWorkLegacy;
        }

        public async Task<IEnumerable<SupplierResponseModel>> GetList(int shopId)
        {
            var suppliers = _context.Suppliers;
            if (_configuration.GetValue<bool>("OwnerGoodForShops"))
                suppliers.Where(s => s.ShopId==shopId);
            return await suppliers.AsNoTracking().OrderBy(s=>s.Name).Select(s => new SupplierResponseModel
            {
                Id = s.Id,
                ShopId = s.ShopId,
                Name = s.Name
            }).ToListAsync();
        }

        public async Task<SupplierResponseModel> Get(int id) => _mapper.Map<SupplierResponseModel>(await _context.Suppliers.FindAsync(id));

        public async Task<SupplierResponseModel> Add(int shopId, SupplierResponseModel model)
        {
            var supplier = _mapper.Map<Supplier>(model);
            supplier.ShopId = shopId;
            var entity = _context.Suppliers.Add(supplier);
            await saveChangesLegacy(entity);
            await _context.SaveChangesAsync();
            return _mapper.Map<SupplierResponseModel>(supplier);
        }

        public async Task<SupplierResponseModel> Update(SupplierResponseModel model)
        {
            var supplier = await _context.Suppliers.FindAsync(model.Id);
            var entity = _context.Entry(supplier);
            _context.ChangeEntityByDTO<SupplierResponseModel>(entity, model);
            await saveChangesLegacy(entity);
            await _context.SaveChangesAsync();
            return _mapper.Map<SupplierResponseModel>(supplier);
        }

        public async Task Delete(int id)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            var entity = _context.Remove(supplier);
            await saveChangesLegacy(entity);
            await _context.SaveChangesAsync();
        }

        private async Task saveChangesLegacy(EntityEntry entity)
        {
            var supplier = entity.Entity as Supplier;
            var shop = await _context.Shops.Where(s => s.Id== supplier.ShopId).FirstOrDefaultAsync();
            if (shop?.LegacyDbNum == null)
                return;
            var supplierLeagcy = _mapper.Map<SupplierLegacy>(supplier);
            supplierLeagcy.Id = supplier.LegacyId ?? 0;
            _unitOfWorkLegacy.SetConnectionString(_configuration.GetConnectionString("shop" + shop.LegacyDbNum));
            if (entity.State == EntityState.Added)
                supplier.LegacyId = await _unitOfWorkLegacy.SupplierRepository.AddAsync(supplierLeagcy);
            if (entity.State == EntityState.Modified)
                await _unitOfWorkLegacy.SupplierRepository.UpdateAsync(supplierLeagcy);
            if (entity.State == EntityState.Deleted)
                await _unitOfWorkLegacy.SupplierRepository.DeleteAsync(supplierLeagcy.Id);
        }
    }
}
