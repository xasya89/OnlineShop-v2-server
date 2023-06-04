using AutoMapper.Configuration.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShop2.LegacyDb.Models
{
    public class GoodCountBalanceCurrentLegacy
    {
        public int GoodId { get; set; }
        public decimal Count { get; set; }
        public decimal CurrentCount { get => Count; }
    }
}
