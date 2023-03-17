﻿using OnlineShop2.Api.Extensions;
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

        public async Task Start(int shopId, int shopNumLegacy)
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
            await _context.SaveChangesAsync();
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
    }
}
