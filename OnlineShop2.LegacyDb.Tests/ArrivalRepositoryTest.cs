using Dapper;
using MySql.Data.MySqlClient;
using OnlineShop2.LegacyDb.Models;
using OnlineShop2.LegacyDb.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShop2.LegacyDb.Tests
{
    internal class ArrivalRepositoryTest
    {
        private string _connectionString = "server=172.172.172.150;database=shop7;uid=xasya;pwd=kt38hmapq";
        private ArrivalRepositoryLegacy _arrivalRepositoryLegacy;
        private GoodRepositoryLegacy _goodRepositoryLegacy;
        private SupplierRepositoryLegacy _supplierRepositoryLegacy;
        [SetUp]
        public void Setup()
        {
            _arrivalRepositoryLegacy = new ArrivalRepositoryLegacy();
            _arrivalRepositoryLegacy.SetConnectionString(_connectionString);
            _goodRepositoryLegacy = new GoodRepositoryLegacy();
            _goodRepositoryLegacy.SetConnectionString(_connectionString);
            _supplierRepositoryLegacy = new SupplierRepositoryLegacy();
            _supplierRepositoryLegacy.SetConnectionString(_connectionString);
        }

        [Test]
        public async Task CreateArrival()
        {
            using(var con= new MySqlConnection(_connectionString))
            {
                await con.OpenAsync();
                await con.ExecuteAsync("UPDATE goodcountbalancecurrents SET Count=0");

                var suppliers = await _supplierRepositoryLegacy.GetAllAsync();
                var goods = await _goodRepositoryLegacy.GetAllAsync();
                var newArrivalGoods = new List<ArrivalGoodLegacy>();
                for (int i = 0; i < 20; i++)
                    newArrivalGoods.Add(
                    new ArrivalGoodLegacy
                    {
                        GoodId = goods[new Random().Next(0, goods.Count - 1)].Id,
                        Count = new Random().Next(1, 10),
                        Price = 10,
                        PriceSell = 100,
                        Nds = ""
                    });
                var newArrival = new ArrivalLegacy
                {
                    Num = "_test_1",
                    DateArrival = DateTime.Now,
                    SupplierId = suppliers[new Random().Next(0, suppliers.Count - 1)].Id,
                    ShopId = 1,
                    SumArrival = newArrivalGoods.Sum(x => x.Price * x.Count),
                    SumPayments = newArrivalGoods.Sum(x => x.Price * x.Count),
                    SumNds = 0,
                    SumSell = newArrivalGoods.Sum(x => x.PriceSell * x.Count),
                    ArrivalGoods = newArrivalGoods
                };
                var id = await _arrivalRepositoryLegacy.AddAsync(newArrival);

                var goodsId = newArrivalGoods.GroupBy(x => x.GoodId).Select(x => x.Key);
                var priceDiff = await con.QuerySingleAsync<int>($"SELECT COUNT(*) FROM goodprices WHERE GoodId IN ({string.Join(',', goods)}) AND Price<>100");
                Assert.AreEqual(priceDiff, 0);
                if (priceDiff != 0)
                    Assert.Fail("Не установлена цена товара");

                var balances = newArrivalGoods.GroupBy(x => x.GoodId).ToDictionary(x => x.Key, x => x.Sum(x => x.Count));
                foreach(var balance in balances)
                {
                    var count = await con.QuerySingleAsync<decimal>("SELECT Count FROM goodcountbalancecurrents WHERE GoodId=" + balance.Key);
                    if (balance.Value != count)
                        Assert.Fail("Не соответствует баланс");
                }
            }
        }
    }
}
