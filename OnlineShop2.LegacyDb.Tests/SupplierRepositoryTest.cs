using OnlineShop2.LegacyDb.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShop2.LegacyDb.Tests
{
    internal class SupplierRepositoryTest
    {
        private SupplierRepositoryLegacy _supplierRepository;

        [SetUp]
        public void Steup()
        {
            _supplierRepository=new SupplierRepositoryLegacy();
            _supplierRepository.SetConnectionString("server=172.172.172.150;database=shop7;uid=xasya;pwd=kt38hmapq");
        }

        [Test]
        public async Task GetSupplierTest()
        {
            for(int i=0; i<=100_000; i++)
            {
                Assert.IsTrue((await _supplierRepository.GetAllAsync()).Any());
            }
        }
    }
}
