using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
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

    public OnlineShopContext()
    {
    }

    public OnlineShopContext(DbContextOptions<OnlineShopContext> options)
        : base(options)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }
    /*
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Server=172.172.172.214;Port=5432;Database=online_shop2;User Id=postgres;Password=312301001;");
        //=> optionsBuilder.UseNpgsql("Server=localhost;Port=5432;Database=sanina;User Id=postgres;Password=kt38hmapq;");
    */
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
