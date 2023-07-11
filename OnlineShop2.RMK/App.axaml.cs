using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OnlineShop2.RMK.Extensions;
using OnlineShop2.RMK.Pages.Rmk;
using OnlineShop2.RMK.ViewModels;
using OnlineShop2.RMK.Views;
using System.IO;

namespace OnlineShop2.RMK
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            IHostBuilder builder = new HostBuilder()
                .ConfigureAppConfiguration(x=>x.AddJsonFile("appsettings.json", optional: true))
                .ConfigureServices((context, services) =>
                {
                    services.AddSerilog()
                    .AddSqlLiteContext();
                });
            Program.Host= builder.Build();

            using (var scope = Program.Host.Services.CreateScope())
            using (var context = scope.ServiceProvider.GetRequiredService<RmkDbContext>())
                context.Database.Migrate();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = ViewModelBase.Create<MainWindowViewModel>(),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}