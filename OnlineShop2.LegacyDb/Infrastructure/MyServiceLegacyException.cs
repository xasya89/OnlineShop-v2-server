using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShop2.LegacyDb.Infrastructure
{
    public class MyServiceLegacyException:Exception
    {
        public MyServiceLegacyException() { }
        public MyServiceLegacyException(string message) : base(message) { }
        public MyServiceLegacyException(string message,  Exception innerException) : base(message, innerException) { }
    }
}
