using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShop2.RMK.Extensions
{
    internal static class DBContextExtension
    {
        public static IServiceCollection AddSqlLiteContext(this IServiceCollection services)
        {

            if (!Directory.Exists("db")) Directory.CreateDirectory("db");
            services.AddDbContext<RmkDbContext>(x => x.UseSqlite("Filename=" + Path.Combine("db", "rmk.db")));
            return services;
        }
    }
}
