using Microsoft.EntityFrameworkCore;
using OnlineShop2.Api.Extensions;
using OnlineShop2.Api.Models.Inventory;
using OnlineShop2.Database;
using OnlineShop2.Database.Models;
using OnlineShop2.LegacyDb;
using OnlineShop2.LegacyDb.Models;
using OnlineShop2.LegacyDb.Repositories;
using System.Linq.Expressions;

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

        public async Task<IEnumerable<InventoryResponseModel>> GetList(int shopId) =>
            MapperConfigurationExtension.GetMapper().Map<IEnumerable<InventoryResponseModel>>(
                await _context.Inventories.Where(i=>i.ShopId==shopId).OrderByDescending(i=>i.Start).ToListAsync()
                );

        public async Task<dynamic> Start(int shopId, int shopNumLegacy, InventoryStartRequestModel model)
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
                ShopId = shopId,
                CashMoneyFact= model.CashMoney
            };
            _context.Inventories.Add(inventory);
            var curGoods = _context.GoodCurrentBalances.Include(b=>b.Good).Where(b => !b.Good.IsDeleted).AsNoTracking();
            foreach (var cur in curGoods)
                _context.Add(new InventorySummaryGood
                {
                    Inventory = inventory,
                    Price = cur.Good.Price,
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
            if (inventory.Status == DocumentStatus.New)
                inventory.InventoryGroups = await _context.InventoryGroups
                    .Include(g => g.InventoryGoods)
                    .ThenInclude(g => g.Good)
                    .Where(g => g.InventoryId == id).AsNoTracking().ToListAsync();
            else
                inventory.InventorySummaryGoods = await _context.InventorySummaryGoods.Include(s => s.Good)
                    .Where(s => s.InventoryId == id).ToListAsync();
            var result = MapperConfigurationExtension.GetMapper().Map<InventoryResponseModel>(inventory);
            return result;
        }

        public async Task<InventoryResponseModel> GetInventoryComplite(int shopId, int id, string? search, int page=0, int pageSize=10000, bool isDiff=true)
        {
            var inventory = await _context.Inventories.Where(i => i.ShopId == shopId & i.Id == id).FirstOrDefaultAsync();
            if (inventory == null)
                throw new MyServiceException("Инвертирозация не найдена");
            int total = await _context.InventorySummaryGoods.Where(i => i.InventoryId == id)
                .Where(s => s.InventoryId == id & (
                    (string.IsNullOrEmpty(search) & isDiff & s.CountCurrent - s.CountOld != 0) ||
                    (!isDiff & !string.IsNullOrEmpty(search) & EF.Functions.Like(s.Good.Name, $"%{search}%")) ||
                    (!string.IsNullOrEmpty(search) & EF.Functions.Like(s.Good.Name, $"%{search}%") & isDiff & s.CountCurrent - s.CountOld != 0) ||
                    (!isDiff & string.IsNullOrEmpty(search))
                )).CountAsync();
            inventory.InventorySummaryGoods = await _context.InventorySummaryGoods.Include(s => s.Good)
                .Where(s => s.InventoryId == id & ( 
                    (string.IsNullOrEmpty(search) & isDiff & s.CountCurrent-s.CountOld != 0) || 
                    (!isDiff & !string.IsNullOrEmpty(search) & EF.Functions.Like(s.Good.Name, $"%{search}%")) || 
                    (!string.IsNullOrEmpty(search) & EF.Functions.Like(s.Good.Name, $"%{search}%") & isDiff & s.CountCurrent - s.CountOld != 0) ||
                    (!isDiff & string.IsNullOrEmpty(search))
                ))
                .Skip((page-1)*pageSize).Take(pageSize).ToListAsync();
            var result = MapperConfigurationExtension.GetMapper().Map<InventoryResponseModel>(inventory);
            result.Total = total;
            return result;
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
            if(inventory.Status==DocumentStatus.Complited)
                throw new MyServiceException("Инвертирозация завершена");
            var newGroup = new InventoryGroup { Inventory=inventory, Name= model.Name };
            _context.InventoryGroups.Add(newGroup);
            await _context.SaveChangesAsync();
            return MapperConfigurationExtension.GetMapper().Map<InventoryGroupResponseModel>(newGroup);
        }

        public async Task<InventoryGroupResponseModel> EditGroup(int groupId, InventoryAddGroupRequestModel model)
        {
            var group = await _context.InventoryGroups.FindAsync(groupId);
            group.Name = model.Name;
            await _context.SaveChangesAsync();
            return MapperConfigurationExtension.GetMapper().Map<InventoryGroupResponseModel>(group);
        }

        public async Task<IEnumerable<InventoryGoodResponseModel>> AddEditGood(int inventoryId, IEnumerable<InventoryAddGoodRequestModel> model)
        {
            var inventory = await _context.Inventories.FindAsync(inventoryId);
            if (inventory == null)
                throw new MyServiceException("Инвертирозация не найдена");
            if (inventory.Status == DocumentStatus.Complited)
                throw new MyServiceException("Инвертирозация завершена");
            var responseModel = await saveChangedGoods(inventoryId, model);

            var transaction = await _context.Database.BeginTransactionAsync();
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return MapperConfigurationExtension.GetMapper().Map<IEnumerable<InventoryGoodResponseModel>>(responseModel); ;
        }
        
        public async Task RemoveGroup(int groupId)
        {
            var group = await _context.InventoryGroups.FirstOrDefaultAsync(gr => gr.Id == groupId);
            if (group==null)
                throw new MyServiceException($"Группа с id {groupId} не найдена");
            _context.Remove(group);
            await _context.SaveChangesAsync();
        }

        public async Task Complite(int id, IEnumerable<InventoryAddGoodRequestModel> model)
        {
            await saveChangedGoods(id, model);
            var inventory = await _context.Inventories
                .Include(i => i.InventoryGroups).ThenInclude(gr => gr.InventoryGoods)
                .Where(i => i.Id == id).FirstAsync();
            if (inventory.Status == DocumentStatus.Complited)
                return;
            inventory.Status = DocumentStatus.Complited;
            inventory.Stop = DateTime.Now;
            var goodsCountFactory = inventory.InventoryGroups
                .SelectMany(gr => gr.InventoryGoods).GroupBy(g => g.GoodId)
                .Select(g => new { GoodId = g.Key, countFact = g.Sum(g => g.CountFact) });
            var inventoryBalanceList = await _context.InventorySummaryGoods.Where(i => i.InventoryId == id).ToListAsync();
            foreach (var goodCountFactory in goodsCountFactory)
                inventoryBalanceList.Where(b => b.GoodId == goodCountFactory.GoodId).First().CountCurrent = goodCountFactory.countFact ?? 0;

            var balanceList = await _context.GoodCurrentBalances.ToListAsync();
            foreach (var balance in balanceList)
                balance.CurrentCount = goodsCountFactory.Where(g => g.GoodId == balance.GoodId).FirstOrDefault()?.countFact ?? 0;

            inventory.SumDb = inventoryBalanceList.Sum(i => i.CountOld * i.Price);
            inventory.SumFact = inventoryBalanceList.Sum(i => i.CountCurrent * i.Price);
            var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _context.SaveChangesAsync();
                var shop = await _context.Shops.FindAsync(inventory.ShopId);
                var balanceNotZero = balanceList.Where(b => b.CurrentCount != 0).ToList();
                using (var unitOfWOrkLegacy = new UnitOfWorkLegacy(_configuration.GetConnectionString("shop" + shop.LegacyDbNum)))
                    await unitOfWOrkLegacy.GoodCountCurrentRepository.SetCurrent(balanceList);
                await transaction.CommitAsync();

            }
            catch(Exception e)
            {
                await transaction.RollbackAsync();
                throw new Exception(e.Message);
            }
        }

        private async Task<IEnumerable<InventoryGood>> saveChangedGoods(int inventoryId, IEnumerable<InventoryAddGoodRequestModel> model)
        {
            var responseModel = new List<InventoryGood>();
            foreach (var item in model.Where(m => m.State == InventoryAddGoodRequestState.Add))
            {
                var newInventoryGood = new InventoryGood { InventoryGroupId = item.GroupId, GoodId = item.GoodId, CountFact = item.CountFact };
                responseModel.Add(newInventoryGood);
                _context.Add(newInventoryGood);
            }
            foreach (var item in model.Where(m => m.State == InventoryAddGoodRequestState.Edit))
            {
                var inventoryGood = await _context.InventoryGoods.Include(g => g.Good).Where(i => i.Id == item.id).FirstAsync();
                inventoryGood.CountFact = item.CountFact;
                responseModel.Add(inventoryGood);
            }
            return responseModel;
        }
    }
}
