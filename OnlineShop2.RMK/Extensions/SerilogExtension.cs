using HarfBuzzSharp;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShop2.RMK.Extensions
{
    internal static class SerilogExtension
    {
        public static IServiceCollection AddSerilog(this IServiceCollection services)
        {
            if (!Directory.Exists("logs"))
                Directory.CreateDirectory("logs");
            var logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File(Path.Combine("logs", "log-.txt"), rollingInterval: RollingInterval.Day
        /*outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"*/
        )
                .CreateLogger();
            services.AddLogging(x => x.AddSerilog(logger: logger, dispose: true));
            return services;
        }
    }
}
