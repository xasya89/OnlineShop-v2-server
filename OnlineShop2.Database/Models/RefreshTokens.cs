using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShop2.Database.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public User User { get; set; }
        public Guid Token { get; set; } = Guid.NewGuid();
        public DateTime Created { get; set; } = DateTime.Now;
    }
}
