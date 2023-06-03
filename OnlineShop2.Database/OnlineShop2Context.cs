using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Configuration;
using OnlineShop2.Database.Models;

namespace OnlineShop2.Database;

public partial class OnlineShopContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Shop> Shops { get; set; }
    public DbSet<Good> Goods { get; set; }
    public DbSet<GoodGroup> GoodsGroups { get; set; }
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<GoodPrice> GoodPrices { get; set; }
    public DbSet<Barcode> Barcodes { get; set; }

    public DbSet<GoodCurrentBalance> GoodCurrentBalances { get; set; }

    public DbSet<Inventory> Inventories { get; set; }
    public DbSet<InventoryGroup> InventoryGroups { get; set; }
    public DbSet<InventoryGood> InventoryGoods { get; set; }
    public DbSet<InventorySummaryGood> InventorySummaryGoods { get; set; }
    public DbSet<InventoryAppendCheck> InventoryAppendChecks { get; set; }

    public DbSet<Shift> Shifts { get; set; }
    public DbSet<ShiftSummary> ShiftSummaries { get; set; }
    public DbSet<CheckSell> CheckSells { get; set; }
    public DbSet<CheckGood> CheckGoods { get; set; }

    public DbSet<Arrival> Arrivals { get; set; }
    public DbSet<ArrivalGood> ArrivalGoods { get; set; }

    public DbSet<Writeof> Writeofs { get; set; }
    public DbSet<WriteofGood> WriteofGoods { get; set; }

    public DbSet<Revaluation> Revaluations { get; set; }
    public DbSet<RevaluationGood> RevaluationGoods { get; set; }

    public DbSet<MoneyReport> MoneyReports { get; set; }

    public OnlineShopContext()
    {
    }

    public OnlineShopContext(DbContextOptions<OnlineShopContext> options)
        : base(options)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }

    //private IUnitOfWorkLegacy 
    /*
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Server=172.172.172.150;Port=5432;Database=online_shop2;User Id=postgres;Password=312301001;");
        //=> optionsBuilder.UseNpgsql("Server=localhost;Port=5432;Database=sanina;User Id=postgres;Password=kt38hmapq;");
    */
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    public void ChangeEntityByDTO<T>(EntityEntry entity, T model)
    {
        string[] collectionsName = new[] { nameof(IList), nameof(ICollection), nameof(IEnumerable) };
        var propertes = typeof(T).GetProperties().Where(p => p.Name.ToLower() != "id");
        foreach (var name in collectionsName)
            propertes = propertes.Where(p => p.PropertyType == typeof(string) || p.PropertyType.GetInterface(name) == null);
        foreach (var prop in propertes)
        {
            var entittyProp = entity.Metadata.GetProperties().Where(p => p.Name == prop.Name).FirstOrDefault();
            if (entittyProp != null)
                entity.Property(entittyProp.Name).CurrentValue = prop.GetValue(model);
        }
    }
}
