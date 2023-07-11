using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShop2.RMK
{
    internal class RmkDbContext:DbContext
    {
        public RmkDbContext()
        {
        }
        public RmkDbContext(DbContextOptions options) : base(options) { }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename="+Path.Combine("db","rmk.db"));
        }
    }
}
