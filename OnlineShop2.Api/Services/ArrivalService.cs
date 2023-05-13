using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using OnlineShop2.Api.BizLogic;
using OnlineShop2.Api.Extensions;
using OnlineShop2.Api.Models.Arrival;
using OnlineShop2.Database;
using OnlineShop2.Database.Models;
using OnlineShop2.LegacyDb.Models;
using OnlineShop2.LegacyDb.Repositories;
using System.Collections;

namespace OnlineShop2.Api.Services
{
    public class ArrivalService
    {
        private readonly ILogger<ArrivalService> _logger;
        public readonly IConfiguration _configuration;
        private readonly OnlineShopContext _context;
        private readonly IMapper _mapper;
        private readonly IUnitOfWorkLegacy _unitOfWorkLegacy;

        public ArrivalService(ILogger<ArrivalService> logger, IConfiguration configuration, OnlineShopContext context, IMapper mapper, IUnitOfWorkLegacy unitOfWorkLegacy)
        {
            _logger = logger;
            _configuration = configuration;
            _context = context;
            _mapper = mapper;
            _unitOfWorkLegacy = unitOfWorkLegacy;
        }

        public async Task<ArrivalSummaryResponseModel[]> GetArrivals(int page, int count, int shopId, int? supplierId) =>
            await _context.Arrivals
            .Include(a => a.Supplier)
            .Where(a => a.ShopId==shopId & (supplierId == null || a.SupplierId == supplierId))
            .OrderByDescending(a=>a.DateArrival).Skip(page).Take(count)
            .Select(a=>new ArrivalSummaryResponseModel
            {
                Id=a.Id,
                Status=a.Status,
                Num=a.Num,
                DateArrival=a.DateArrival,
                SupplierId=a.SupplierId,
                SupplierName=a.Supplier.Name,
                ShopId=a.ShopId,
                PurchaseAmount=a.PurchaseAmount,
                SumNds=a.SumNds,
                SaleAmount=a.SaleAmount,
                LegacyId=a.LegacyId
            })
            .ToArrayAsync();

        public async Task<ArrivalResponseModel> GetOne(int id) =>
            _mapper.Map<ArrivalResponseModel>(await _context.Arrivals.Include(a => a.ArrivalGoods).ThenInclude(a => a.Good).AsNoTracking().FirstAsync(a => a.Id == id));
            

        public async Task<ArrivalModel> Create(int shopId, ArrivalModel model)
        {
            if (model.Id != 0)
                throw new MyServiceException("Невозможно создать повторно сущестующий документ прихода");
            var arrival = _mapper.Map<Arrival>(model);
            var entity = _context.Add(arrival);
            await operationPriceBalanceChange(entity);

            await legacySaveChange(entity);
            await _context.SaveChangesAsync();
            int i = 0;
            foreach (var agood in arrival.ArrivalGoods)
                model.ArrivalGoods[i++].Id = agood.Id;

            return _mapper.Map<ArrivalModel>(arrival);
        }

        public async Task<ArrivalModel> Edit(ArrivalModel model)
        {
            var arrival = _mapper.Map<Arrival>(model);
            var entity = _context.Arrivals.Update(arrival);
            var isDeleteIds = model.ArrivalGoods.Where(a => a.IsDelete).Select(a => a.Id);
            arrival.ArrivalGoods.Where(a => isDeleteIds.Contains(a.Id)).ToList().ForEach(a => _context.ArrivalGoods.Entry(a).State = EntityState.Deleted);
            await operationPriceBalanceChange(entity);
            await legacySaveChange(entity);
            await _context.SaveChangesAsync();
            for (int i = 0; i < model.ArrivalGoods.Count; i++)
                model.ArrivalGoods[i].Id = arrival.ArrivalGoods[i].Id;
            return model;
        }

        public async Task Remove(int id)
        {
            _context.Remove(new Arrival { Id = id });
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Изменим количество и цену товара после созранения
        /// </summary>
        /// <param name="entity">Сущность прихода</param>
        /// <param name="isDeletePositionsId">Список позиций помеченных на удаление</param>
        /// <returns></returns>
        private async Task operationPriceBalanceChange(EntityEntry entity, IEnumerable<int> isDeletePositionsId = null)
        {
            var arrival = entity.Entity as Arrival;
            //Вычтем количество указаное переж редактированием
            if(entity.State==EntityState.Modified)
            {
                var prevArrivalGoods = await _context.ArrivalGoods.Where(a => a.ArrivalId == arrival.Id).AsNoTracking().ToListAsync();
                await CurrentBalanceChange.Change(_context, arrival.ShopId, prevArrivalGoods.GroupBy(x => x.GoodId).ToDictionary(a => a.Key, a => -1 * a.Sum(x => x.Count)));
            }

            var arrivalgoods = arrival.ArrivalGoods;
            //Исключим позиции помеченные на удаление
            if (isDeletePositionsId != null)
                arrivalgoods = arrivalgoods.Where(a => !isDeletePositionsId.Contains(a.Id)).ToList();

            var arrivalgoodsGroups = arrivalgoods.GroupBy(a=>a.GoodId)
                .Select(a=>new { GoodId = a.Key, PriceSell = a.First().PriceSell, Count = a.Sum(x=>x.Count) });
            //Изменим прайс
            await PriceChange.Change(_context, arrival.ShopId, arrivalgoodsGroups.ToDictionary(x => x.GoodId, x => x.PriceSell));
            //Добавим уоличество
            await CurrentBalanceChange.Change(_context, arrival.ShopId, arrivalgoodsGroups.ToDictionary(x => x.GoodId, x => x.Count));
        }

        /// <summary>
        /// Изменяет кол-во и цену в legacy бд
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="deletePositionsId"></param>
        /// <returns></returns>
        private async Task legacySaveChange(EntityEntry entity, IEnumerable<int> deletePositionsId = null)
        {
            var arrival = entity.Entity as Arrival;
            var shop = await _context.Shops.FindAsync(arrival.ShopId);
            if (shop.LegacyDbNum == null)
                return;
            if(arrival.LegacyId==null) return;
            _unitOfWorkLegacy.SetConnectionString(_configuration.GetConnectionString("shop" + shop.LegacyDbNum));

            if (entity.State == EntityState.Deleted)
            {
                await _unitOfWorkLegacy.ArrivalRepository.DeleteAsync((int)arrival.LegacyId);
                return;
            }    

            var arrivalLegacy = _mapper.Map<ArrivalLegacy>(arrival);
            arrivalLegacy.Id = (int)arrival.LegacyId;
            arrivalLegacy.ShopId = 1;
            arrivalLegacy.SupplierId = (int)(await _context.Suppliers.FindAsync(arrival.SupplierId)).LegacyId;
            var goodsId = arrival.ArrivalGoods.Select(x => x.GoodId);
            var goods = await _context.Goods.Where(g => goodsId.Contains(g.Id)).AsNoTracking().ToListAsync();
            foreach (var goodLegacy in arrivalLegacy.ArrivalGoods)
                goodLegacy.GoodId = (int)goods.First(x => x.Id == goodLegacy.GoodId).LegacyId;

            //Уберем помеченные на удаление строки
            if (deletePositionsId != null)
                arrivalLegacy.ArrivalGoods = arrivalLegacy.ArrivalGoods.Where(a => !deletePositionsId.Contains(a.Id)).ToList();

            if (entity.State==EntityState.Added)
                arrival.LegacyId= await _unitOfWorkLegacy.ArrivalRepository.AddAsync(arrivalLegacy);
            if(entity.State==EntityState.Modified)
                await _unitOfWorkLegacy.ArrivalRepository.UpdateAsync(arrivalLegacy);
        }
    }
}
