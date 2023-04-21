using OnlineShop2.LegacyDb.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShop2.LegacyDb.Tests
{
    internal class ShiftRepositoryTest
    {
        private ShiftRepositoryLegacy _repository;
        [SetUp]
        public void Setup()
        {
            _repository = new ShiftRepositoryLegacy();
            _repository.SetConnectionString("server=172.172.172.150;database=shop7;uid=xasya;pwd=kt38hmapq");
        }

        [TestCase("10.01.2023")]
        [TestCase("15.04.2023")]
        [TestCase("20.04.2023")]
        public async Task CheckRecieptCashShift(string withStr)
        {
            DateOnly with = DateOnly.Parse(withStr);
            var shifts = await _repository.GetShifts(with);
            Assert.IsTrue(shifts.Any());
            Assert.IsTrue(shifts.SelectMany(s=>s.CheckSells).Any());
            Assert.IsTrue(shifts.SelectMany(s => s.CheckSells.SelectMany(c => c.CheckGoods)).Count() > 0);
        }
    }
}
