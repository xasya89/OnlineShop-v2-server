using Microsoft.EntityFrameworkCore;
using OnlineShop2.Api.Extensions;
using OnlineShop2.Api.Models.Arrival;
using OnlineShop2.Database;
using OnlineShop2.Database.Models;
using System.Collections;

namespace OnlineShop2.Api.Services
{
    public class ArrivalService
    {
        private readonly ILogger<ArrivalService> _logger;
        public readonly IConfiguration _configuration;
        private readonly OnlineShopContext _context;

        public ArrivalService(ILogger<ArrivalService> logger, IConfiguration configuration, OnlineShopContext context)
        {
            _logger = logger;
            _configuration = configuration;
            _context = context;
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

        public async Task<ArrivalRequestModel> GetOne(int id) =>
            await _context.Arrivals.Include(a => a.Supplier).Include(a => a.ArrivalGoods).ThenInclude(a => a.Good).Where(a => a.Id == id)
            .Select(a => new ArrivalRequestModel
            {
                Id = a.Id,
                Status = a.Status,
                Num = a.Num,
                DateArrival = a.DateArrival,
                SupplierId = a.SupplierId,
                SupplierName = a.Supplier.Name,
                ShopId = a.ShopId,
                LegacyId = a.LegacyId,
                Positions=a.ArrivalGoods.Select(a=>new ArrivalGoodRequestModel
                {
                    Id=a.Id,
                    SequenceNum=a.SequenceNum,
                    GoodId=a.GoodId,
                    GoodName=a.Good.Name,
                    PricePurchase=a.PricePurchase,
                    Nds=a.Nds,
                    PriceSell=a.PriceSell,
                    Count=a.Count,
                    ExpiresDate=a.ExpiresDate
                }).ToList()
            }).FirstAsync();
            

        public async Task<ArrivalRequestModel> Create(ArrivalRequestModel model)
        {
            if (model.Id != 0)
                throw new MyServiceException("Невозможно создать повторно сущестующий документ прихода");
            var arrival = new Arrival
            {
                Status = model.Status,
                Num = model.Num,
                DateArrival = model.DateArrival,
                SupplierId = model.SupplierId,
                ShopId = model.ShopId,
                PurchaseAmount = model.Positions.Sum(a => a.PricePurchase * a.Count),
                SumNds = 0,
                SaleAmount = model.Positions.Sum(a => a.PriceSell * a.Count),
                ArrivalGoods=model.Positions.Select(a=>new ArrivalGood
                {
                    GoodId=a.GoodId,
                    Count=a.Count,
                    PricePurchase=a.PricePurchase,
                    PriceSell=a.PriceSell,
                    ExpiresDate=a.ExpiresDate
                }).ToList()
            };
            _context.Add(arrival);
            await _context.SaveChangesAsync();
            var supplier = await _context.Suppliers.FindAsync(arrival.SupplierId);
            int i = 0;
            foreach (var agood in arrival.ArrivalGoods)
                model.Positions[i++].Id = agood.Id;

            return model;
        }

        public async Task<ArrivalRequestModel> Edit(ArrivalRequestModel model)
        {
            var arrival = await _context.Arrivals.FindAsync(model.Id);
            if (arrival == null)
                throw new MyServiceException($"Приход с id {model.Id} не найдена");
            arrival.Status = model.Status;
            arrival.Num = model.Num;
            arrival.DateArrival = model.DateArrival;
            arrival.SupplierId = model.SupplierId;
            arrival.ShopId = model.ShopId;
            arrival.PurchaseAmount = model.Positions.Sum(a => a.PricePurchase * a.Count);
            arrival.SumNds = 0;
            arrival.SaleAmount=model.Positions.Sum(a=>a.PriceSell* a.Count);
            arrival.LegacyId = model.LegacyId;

            var newPositions = model.Positions.Where(p => p.Id == 0).Select(p => new ArrivalGood
            {
                ArrivalId = arrival.Id,
                SequenceNum=p.SequenceNum,
                GoodId = p.GoodId,
                PricePurchase = p.PricePurchase,
                Nds = p.Nds,
                PriceSell = p.PriceSell,
                Count = p.Count,
                ExpiresDate = p.ExpiresDate
            });
            _context.ArrivalGoods.AddRange(newPositions);

            foreach (var position in model.Positions.Where(p => p.Id != 0))
            {
                var arrivalGood = await _context.ArrivalGoods.FindAsync(position.Id);
                arrivalGood.SequenceNum=position.SequenceNum;
                arrivalGood.GoodId = position.GoodId;
                arrivalGood.Count = position.Count;
                arrivalGood.PricePurchase= position.PricePurchase;
                arrivalGood.Nds = position.Nds;
                arrivalGood.PriceSell = position.PriceSell;
                arrivalGood.ExpiresDate = position.ExpiresDate;
            }
            await _context.SaveChangesAsync();

            foreach (var pos in newPositions)
                model.Positions.Where(p => p.SequenceNum == pos.SequenceNum & p.Id == 0).First().Id = pos.Id;
            return model;
        }

        public async Task Remove(int id)
        {
            var arrival = await _context.Arrivals.FindAsync(id);
            if (arrival == null)
                throw new MyServiceException($"Приход с id {id} не найдена");
            _context.Remove(arrival);
            await _context.SaveChangesAsync();
        }
    }
}
