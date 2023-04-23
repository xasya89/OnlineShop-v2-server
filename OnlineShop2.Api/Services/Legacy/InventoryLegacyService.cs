using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
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
        private readonly IMapper _mapper;
        private readonly IUnitOfWorkLegacy _unitOfWork;
        private string inventoryShema = "";

        private static string INVENTORY_SHEMA_COUNT_ON_START = "CurrentBalanceOnStart";
        private static string INVENTORY_SHEMA_AFTER_CLOSE = "GetBalanceAfterCloseShift";
        public InventoryLegacyService(OnlineShopContext context, IConfiguration configuration, IMapper mapper, IUnitOfWorkLegacy unitOfWork)
        {
            _context = context;
            _configuration = configuration;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            inventoryShema = configuration.GetSection("InventoryShema").Value;
        }

        public async Task<IEnumerable<InventoryResponseModel>> GetList(int shopId) =>
            _mapper.Map<IEnumerable<InventoryResponseModel>>(
                await _context.Inventories.Where(i=>i.ShopId==shopId).OrderByDescending(i=>i.Start).ToListAsync()
                );

        public async Task<dynamic> Start(int shopId, int shopNumLegacy, InventoryStartRequestModel model)
        {
            if (_context.Inventories.Where(i => i.Status != DocumentStatus.Canceled & i.Status != DocumentStatus.Successed).Count() > 0)
                throw new MyServiceException("Предыдущая инверторизация не завершена");
            var currentDate = DateOnly.FromDateTime(DateTime.Now).ToDateTime(TimeOnly.MinValue);
            var shift = await _context.Shifts.Where(s => s.Start > currentDate & s.Start < currentDate.AddDays(1) & s.Stop == null).FirstOrDefaultAsync();
            if (shift == null)
                throw new MyServiceException("Смена на сеогдняшний день не найдена");
            var inventory = new Inventory
            {
                ShopId = shopId,
                CashMoneyFact = model.CashMoney,
                CurrentShiftId=shift.Id
            };
            _context.Inventories.Add(inventory);

            //Расчет остатков на начало инвенторизации
            if (_configuration.GetSection("InventoryShema").Value == "CurrentBalanceOnStart")
            {
                if (shift != null)
                    throw new MyServiceException("Есть не закрытая смена");
                _unitOfWork.SetConnectionString(_configuration.GetConnectionString("shop" + shopNumLegacy));
                var repository = _unitOfWork.CurrentBalance;
                await synchBalance(repository, shopId); 
                var curGoods = _context.GoodCurrentBalances.Include(b => b.Good).Where(b => !b.Good.IsDeleted).AsNoTracking();
                foreach (var cur in curGoods)
                    _context.Add(new InventorySummaryGood
                    {
                        Inventory = inventory,
                        Price = cur.Good.Price,
                        GoodId = cur.GoodId,
                        CountOld = cur.CurrentCount
                    });
            }
            if (_configuration.GetSection("InventoryShema").Value == "GetBalanceAfterCloseShift")
            {
                if(shift==null)
                    throw new MyServiceException("Нет открытой смены");
                inventory.CashMoneyFact = null;
            }
            await _context.SaveChangesAsync();
            return new { id = inventory.Id };
        }

        private async Task synchBalance(ICurrentBalanceRepositoryLegacy repository, int shopId)
        {
            var legacy = await repository.GetCurrent();
            var goods = await _context.Goods.AsNoTracking().ToListAsync();
            var balanceCurrent = await _context.GoodCurrentBalances.Include(b => b.Good).Where(b => b.ShopId == shopId).AsNoTracking().ToListAsync();
            var rowsAdd = from good in goods
                          join balanceLegacy in (
                             from balanceLegacy in legacy
                             join balance in balanceCurrent
                             on balanceLegacy.GoodId equals balance.Good.LegacyId into t
                             from subBalance in t.DefaultIfEmpty()
                             where subBalance == null
                             select balanceLegacy)
                         on good.LegacyId equals balanceLegacy.GoodId
                          select new GoodCurrentBalance { ShopId = shopId, GoodId = good.Id, CurrentCount = balanceLegacy.CurrentCount };
            await _context.GoodCurrentBalances.AddRangeAsync(rowsAdd);


            var rowsEdit = from balanceLegacy in legacy
                           join balance in balanceCurrent
                           on balanceLegacy.GoodId equals balance.Good.LegacyId into t
                           from subBalance in t.DefaultIfEmpty()
                           where subBalance != null && balanceLegacy.Count != subBalance?.CurrentCount
                           select new { db = subBalance, count = balanceLegacy.CurrentCount };
            foreach (var row in rowsEdit)
            {
                _context.Entry(row.db).State = EntityState.Modified;
                row.db.CurrentCount = row.count;
            }

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
            var result = _mapper.Map<InventoryResponseModel>(inventory);
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
            var result = _mapper.Map<InventoryResponseModel>(inventory);
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
            return _mapper.Map<InventoryGroupResponseModel>(newGroup);
        }

        public async Task<InventoryGroupResponseModel> EditGroup(int groupId, InventoryAddGroupRequestModel model)
        {
            var group = await _context.InventoryGroups.FindAsync(groupId);
            group.Name = model.Name;
            await _context.SaveChangesAsync();
            return _mapper.Map<InventoryGroupResponseModel>(group);
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

            return _mapper.Map<IEnumerable<InventoryGoodResponseModel>>(responseModel); ;
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
            var inventory = await _context.Inventories.Where(i => i.Id == id).FirstAsync();
            if (inventoryShema == INVENTORY_SHEMA_AFTER_CLOSE)
                inventory.Status = DocumentStatus.Processing;
            await _context.SaveChangesAsync();

            /*
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
            //Учитываем чеки которые были проведены за время проведения инвенторизации
            var goodsCheck = await _context.InventoryAppendChecks.Where(i => i.InventoryId == inventory.Id).GroupBy(i => i.GoodId).AsNoTracking()
                .Select(i => new { GoodId = i.Key, Count = i.Sum(x => x.Count) }).ToListAsync();
            foreach(var goodCheck in goodsCheck)
                inventoryBalanceList.Where(b => b.GoodId == goodCheck.GoodId).First().CountCurrent = goodCheck.Count;
            //Установим текущие остатки в бд
            var balanceList = await _context.GoodCurrentBalances.Include(b=>b.Good).AsNoTracking().ToListAsync();
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
                {
                    var setLegacyBalance = balanceList.GroupBy(b => b.Good.LegacyId).Select(b => new { goodLegacyId = b.Key ?? 0, count = b.Sum(x => x.CurrentCount) }).ToDictionary(x=>x.goodLegacyId, x=>x.count);
                    await unitOfWOrkLegacy.GoodCountCurrentRepository.SetCurrent(setLegacyBalance);
                }
                    
                await transaction.CommitAsync();

            }
            catch(Exception e)
            {
                await transaction.RollbackAsync();
                throw new Exception(e.Message);
            }
            */
        }

        public async Task CompliteSetMoneyFact(int id, InventoryStartRequestModel model)
        {
            if (inventoryShema == INVENTORY_SHEMA_COUNT_ON_START)
                throw new MyServiceException("Инвенторизация идет по обычной схеме");
            var inventory = await _context.Inventories.Where(i => i.Id == id).FirstAsync();
            if(inventory.Status==DocumentStatus.Processing)
                throw new MyServiceException("В инвенторизации не получен отчет о закрытии смены");
            inventory.CashMoneyFact = model.CashMoney;
            inventory.Status = DocumentStatus.Successed;
            await _context.SaveChangesAsync();
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
