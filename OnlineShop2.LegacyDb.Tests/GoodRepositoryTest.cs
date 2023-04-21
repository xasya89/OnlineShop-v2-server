using OnlineShop2.Database.Models;
using OnlineShop2.LegacyDb.Models;
using OnlineShop2.LegacyDb.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShop2.LegacyDb.Tests
{
    internal class GoodRepositoryTest
    {
        private GoodRepositoryLegacy _repositoryLegacy; 
        [SetUp]
        public void Setup ()
        {
            _repositoryLegacy = new GoodRepositoryLegacy ();
            _repositoryLegacy.SetConnectionString("server=172.172.172.150;database=shop7;uid=xasya;pwd=kt38hmapq");
        }

        [Test]
        public async Task CreateGood()
        {
            var good = new GoodLegacy
            {
                GoodGroupId = 1,
                Name = "Good test",
                Unit = Units.PCE,
                Price = 100,
                IsDeleted = false,
                GoodPrices = new List<GoodPriceLegacy>()
                {
                    new GoodPriceLegacy{Price=100}
                },
                Barcodes = new List<BarCodeLegacy>()
                {
                    new BarCodeLegacy{Code="123"},
                    new BarCodeLegacy{Code="124"},
                }
            };
            Assert.IsTrue(await _repositoryLegacy.AddAsync(good, 1) > 0);
        }

        [Test]
        public async Task UpdateGood()
        {
            var good = await _repositoryLegacy.GetByIdAsync(1487);
            TestContext.WriteLine(good);
            good.Name = "Good";
            good.Price = 12;
            good.GoodPrices.ForEach(p => p.Price = 12);
            good.GoodPrices.Add(new GoodPriceLegacy { GoodId = good.Id, Price = 14 });
            await _repositoryLegacy.UpdateAsync(good);
            good = await _repositoryLegacy.GetByIdAsync(1487);
            Assert.IsNotNull(good);
            Assert.IsTrue(good.Name == "Good");
            Assert.IsTrue(good.GoodPrices.Count() == 3);
        }

        [TestCase(1477)]
        [TestCase(1478)]
        [TestCase(1485)]
        [TestCase(1486)]
        public async Task DeleteGood(int id)
        {
            await _repositoryLegacy.DeleteAsync(id);
            Assert.IsNull(await _repositoryLegacy.GetByIdAsync(id));
        }
    }
}
