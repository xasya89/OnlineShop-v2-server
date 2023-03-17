using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShop2.Database.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string UserName { get; set; }
        public UserRole Role { get; set; }
        public bool Active { get; set; } = true;
    }

    public enum UserRole
    {
        Admin, Manager, Cashier
    }
}
