using Microsoft.EntityFrameworkCore;
using OnlineShop2.Api.Extensions;
using OnlineShop2.Api.Models.Inventory;
using OnlineShop2.Database;
using OnlineShop2.Database.Models;
using OnlineShop2.LegacyDb;
using OnlineShop2.LegacyDb.Models;
using OnlineShop2.LegacyDb.Repositories;

namespace OnlineShop2.Api.Services.Legacy
{
    public class InventoryLegacyService
    {
        private readonly OnlineShopContext _context;
        private readonly IConfiguration _configuration;
        public InventoryLegacyService(OnlineShopContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<dynamic> Start(int shopId, int shopNumLegacy)
        {
            if (_context.Inventories.Where(i => i.Status == DocumentStatus.New || i.Status==DocumentStatus.Successed).Count() > 0)
                throw new MyServiceException("Предыдущая инверторизация не завершена");
            using (var unitOfWOrkLegacy = new UnitOfWorkLegacy(_configuration.GetConnectionString("shop" + shopNumLegacy)))
            {
                var repository = unitOfWOrkLegacy.GoodCountCurrentRepository;
                if (!(await repository.ShiftClosedStatus()))
                    throw new MyServiceException("Есть не закрытая смена");
                await synchBalance(repository, shopId);;
            }

            var inventory = new Inventory
            {
                ShopId = shopId
            };
            _context.Inventories.Add(inventory);
            var curGoods = _context.GoodCurrentBalances.Where(b => b.CurrentCount > 0).AsNoTracking();
            foreach (var cur in curGoods)
                _context.Add(new InventorySummaryGood
                {
                    Inventory = inventory,
                    GoodId = cur.GoodId,
                    CountOld = cur.CurrentCount
                });
            await _context.SaveChangesAsync();
            return new { id = inventory.Id };
        }

        private async Task synchBalance(GoodCurrentBalanceLegacyRepository repository, int shopId)
        {
            var legacy = await repository.GetCurrent();
            var rowsAdd = from balanceLegacy in legacy
                         join balance in  _context.GoodCurrentBalances.Where(b=>b.ShopId==shopId)
                         on balanceLegacy.GoodId equals balance.GoodId into t
                         from subBalance in t.DefaultIfEmpty()
                         where subBalance==null
                         select balanceLegacy;
            foreach (var row in rowsAdd)
            {
                row.Id = 0;
                row.ShopId = shopId;
            }
            await _context.GoodCurrentBalances.AddRangeAsync(rowsAdd);
            
            var rowsEdit = from balanceLegacy in legacy
                           join balance in _context.GoodCurrentBalances.Where(b=>b.ShopId==shopId)
                           on balanceLegacy.GoodId equals balance.GoodId into t
                           from subBalance in t.DefaultIfEmpty()
                           where subBalance != null
                           select new { db = subBalance, count = balanceLegacy.CurrentCount };
            foreach (var row in rowsEdit)
                row.db.CurrentCount = row.count;

            await _context.SaveChangesAsync();
        }

        public async Task<InventoryResponseModel> GetInventory(int shopId, int id)
        {
            var inventory = await _context.Inventories.Where(i => i.ShopId == shopId & i.Id == id).FirstOrDefaultAsync();
            if (inventory == null)
                throw new MyServiceException("Инвертирозация не найдена");
            if(inventory.Status==DocumentStatus.New || inventory.Status==DocumentStatus.Canceled)
            {
                var groups = await _context.InventoryGroups
                    .Include(g=>g.InventoryGoods)
                    .ThenInclude(g=>g.Good)
                    .Where(g => g.InventoryId == id).AsNoTracking().ToListAsync();
                inventory.InventoryGroups = groups;
            }
            return MapperConfigurationExtension.GetMapper().Map<InventoryResponseModel>(inventory);
        }

        public async Task RemoveInventory(int shopid, int id)
        {
            var inventory = await _context.Inventories.FirstAsync(i => i.ShopId == shopid & i.Id == id);
            if (inventory == null)
                throw new MyServiceException("Инвенторизация не найдена");
            if (inventory.Status == DocumentStatus.Complited)
                throw new MyServiceException("Инвенторизация уже подтверждена");
            _context.Remove(inventory);
            await _context.SaveChangesAsync();
        }

        public async Task<InventoryGroupResponseModel> AddGroup(int id, InventoryAddGroupRequestModel model)
        {
            var inventory = await _context.Inventories.FindAsync(id);
            if (inventory == null)
                throw new MyServiceException("Инвертирозация не найдена");
            var newGroup = new InventoryGroup { Inventory=inventory, Name= model.Name };
            _context.InventoryGroups.Add(newGroup);
            await _context.SaveChangesAsync();
            return MapperConfigurationExtension.GetMapper().Map<InventoryGroupResponseModel>(newGroup);
        }
    }
}
