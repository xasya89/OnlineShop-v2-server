using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using MySql.Data.MySqlClient;
using OnlineShop2.Database.Models;
using OnlineShop2.LegacyDb.Infrastructure.MapperConfigurations;
using OnlineShop2.LegacyDb.Models;

namespace OnlineShop2.LegacyDb.Repositories
{
    public class GoodLegacyRepository
    {
        private readonly MySqlConnection _connection;
        public GoodLegacyRepository(MySqlConnection connection) => _connection = connection;

        public async Task<IEnumerable<GoodGroup>> GetGroupsAsync() =>
            MapperInstance.GetMapper().Map<IEnumerable<GoodGroupLegacy>, IEnumerable<GoodGroup>>( await _connection.QueryAsync<GoodGroupLegacy>("SELECT * FROM goodgroups"));
        public async Task<IEnumerable<Supplier>> GetSuppliersAsync() =>
            MapperInstance.GetMapper().Map<IEnumerable<SupplierLegacy>, IEnumerable<Supplier>>(await _connection.QueryAsync<SupplierLegacy>("SELECT * FROM suppliers"));
        public async Task<IEnumerable<Good>> GetGoodsAsync()
        {
            var goods = (await _connection.QueryAsync<GoodLegacy>("SELECT * FROM goods")).ToList();
            var prices = await _connection.QueryAsync<GoodPriceLegacy>("SELECT * FROM goodprices");
            var barcodes = await _connection.QueryAsync<BarCodeLegacy>("SELECT * FROM barcodes");
            foreach(var price in prices)
                goods.Find(g => g.Id == price.GoodId)?.GoodPrices.Add(price);
            foreach (var barcode in barcodes)
                goods.Find(g => g.Id == barcode.GoodId)?.Barcodes.Add(barcode);
            return MapperInstance.GetMapper().Map<IEnumerable<GoodLegacy>, IEnumerable<Good>>(goods);
        }

    }
}
