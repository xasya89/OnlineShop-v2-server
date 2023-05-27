using OnlineShop2.LegacyDb.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShop2.LegacyDb.Tests
{
    internal class WriteofRepositoryTest
    {
        private string _connectionString = "server=172.172.172.150;database=shop7_actual;uid=xasya;pwd=kt38hmapq";
        private WriteofRepositoryLegacy writeofRepositoryLegacy;

        [SetUp]
        public void Setup()
        {
            writeofRepositoryLegacy = new WriteofRepositoryLegacy();
            writeofRepositoryLegacy.SetConnectionString(_connectionString);
        }

        [Test]
        public async Task GetWriteofs()
        {
            var writeofs = await writeofRepositoryLegacy.GetWriteOfWithDate(DateTime.Parse("26.05.2023"));
            Assert.IsTrue(writeofs.Count() > 0);
            Assert.IsTrue(writeofs.SelectMany(w => w.WriteofGoods).Count() > 0);
        }
    }
}
