﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using OnlineShop2.Database;

#nullable disable

namespace OnlineShop2.Database.Migrations
{
    [DbContext(typeof(OnlineShopContext))]
    [Migration("20230330101750_add_Shift_with_sother_tables")]
    partial class add_Shift_with_sother_tables
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("OnlineShop2.Database.Models.Barcode", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("GoodId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("GoodId");

                    b.ToTable("Barcodes");
                });

            modelBuilder.Entity("OnlineShop2.Database.Models.CheckGood", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("CheckSellId")
                        .HasColumnType("integer");

                    b.Property<decimal>("Count")
                        .HasColumnType("numeric");

                    b.Property<int>("GoodId")
                        .HasColumnType("integer");

                    b.Property<decimal>("Price")
                        .HasColumnType("numeric");

                    b.HasKey("Id");

                    b.HasIndex("CheckSellId");

                    b.HasIndex("GoodId");

                    b.ToTable("CheckGoods");
                });

            modelBuilder.Entity("OnlineShop2.Database.Models.CheckSell", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int?>("BuyerId")
                        .HasColumnType("integer");

                    b.Property<string>("BuyerName")
                        .HasColumnType("text");

                    b.Property<string>("BuyerPhone")
                        .HasColumnType("text");

                    b.Property<DateTime>("DateCreate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("ShiftId")
                        .HasColumnType("integer");

                    b.Property<decimal>("SumBuy")
                        .HasColumnType("numeric");

                    b.Property<decimal>("SumDiscont")
                        .HasColumnType("numeric");

                    b.Property<decimal>("SumElectron")
                        .HasColumnType("numeric");

                    b.Property<decimal>("SumNoElectron")
                        .HasColumnType("numeric");

                    b.Property<int>("TypeSell")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("ShiftId");

                    b.ToTable("CheckSells");
                });

            modelBuilder.Entity("OnlineShop2.Database.Models.Good", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Article")
                        .HasColumnType("text");

                    b.Property<int>("GoodGroupId")
                        .HasColumnType("integer");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<int?>("LegacyId")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<decimal>("Price")
                        .HasColumnType("numeric");

                    b.Property<int?>("ShopId")
                        .HasColumnType("integer");

                    b.Property<int>("SpecialType")
                        .HasColumnType("integer");

                    b.Property<int?>("SupplierId")
                        .HasColumnType("integer");

                    b.Property<int>("Unit")
                        .HasColumnType("integer");

                    b.Property<Guid>("Uuid")
                        .HasColumnType("uuid");

                    b.Property<double?>("VPackage")
                        .HasColumnType("double precision");

                    b.HasKey("Id");

                    b.HasIndex("GoodGroupId");

                    b.HasIndex("LegacyId");

                    b.HasIndex("ShopId");

                    b.HasIndex("SupplierId");

                    b.ToTable("Goods");
                });

            modelBuilder.Entity("OnlineShop2.Database.Models.GoodCurrentBalance", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<decimal>("CurrentCount")
                        .HasColumnType("numeric");

                    b.Property<int>("GoodId")
                        .HasColumnType("integer");

                    b.Property<int>("ShopId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("GoodId")
                        .IsUnique();

                    b.HasIndex("ShopId");

                    b.ToTable("GoodCurrentBalances");
                });

            modelBuilder.Entity("OnlineShop2.Database.Models.GoodGroup", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int?>("LegacyId")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("ShopId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("LegacyId");

                    b.HasIndex("ShopId");

                    b.ToTable("GoodsGroups");
                });

            modelBuilder.Entity("OnlineShop2.Database.Models.GoodPrice", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int?>("GoodId")
                        .HasColumnType("integer");

                    b.Property<decimal>("Price")
                        .HasColumnType("numeric");

                    b.Property<int>("ShopId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("GoodId");

                    b.HasIndex("ShopId");

                    b.ToTable("GoodPrices");
                });

            modelBuilder.Entity("OnlineShop2.Database.Models.Inventory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<decimal>("CashMoneyDb")
                        .HasColumnType("numeric");

                    b.Property<decimal>("CashMoneyFact")
                        .HasColumnType("numeric");

                    b.Property<int>("ShopId")
                        .HasColumnType("integer");

                    b.Property<DateTime>("Start")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<DateTime?>("Stop")
                        .HasColumnType("timestamp with time zone");

                    b.Property<decimal>("SumDb")
                        .HasColumnType("numeric");

                    b.Property<decimal>("SumFact")
                        .HasColumnType("numeric");

                    b.HasKey("Id");

                    b.HasIndex("ShopId");

                    b.ToTable("Inventories");
                });

            modelBuilder.Entity("OnlineShop2.Database.Models.InventoryGood", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<decimal?>("CountAppend")
                        .HasColumnType("numeric");

                    b.Property<decimal>("CountDB")
                        .HasColumnType("numeric");

                    b.Property<decimal?>("CountFact")
                        .HasColumnType("numeric");

                    b.Property<int>("GoodId")
                        .HasColumnType("integer");

                    b.Property<int>("InventoryGroupId")
                        .HasColumnType("integer");

                    b.Property<decimal>("Price")
                        .HasColumnType("numeric");

                    b.HasKey("Id");

                    b.HasIndex("GoodId");

                    b.HasIndex("InventoryGroupId");

                    b.ToTable("InventoryGoods");
                });

            modelBuilder.Entity("OnlineShop2.Database.Models.InventoryGroup", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("InventoryId")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("InventoryId");

                    b.ToTable("InventoryGroups");
                });

            modelBuilder.Entity("OnlineShop2.Database.Models.InventorySummaryGood", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<decimal>("CountCurrent")
                        .HasColumnType("numeric");

                    b.Property<decimal>("CountOld")
                        .HasColumnType("numeric");

                    b.Property<int>("GoodId")
                        .HasColumnType("integer");

                    b.Property<int?>("InventoryGroupId")
                        .HasColumnType("integer");

                    b.Property<int>("InventoryId")
                        .HasColumnType("integer");

                    b.Property<decimal>("Price")
                        .HasColumnType("numeric");

                    b.HasKey("Id");

                    b.HasIndex("GoodId");

                    b.HasIndex("InventoryGroupId");

                    b.HasIndex("InventoryId");

                    b.ToTable("InventorySummaryGoods");
                });

            modelBuilder.Entity("OnlineShop2.Database.Models.RefreshToken", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("Token")
                        .HasColumnType("uuid");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("RefreshTokens");
                });

            modelBuilder.Entity("OnlineShop2.Database.Models.Shift", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("CashierName")
                        .HasColumnType("text");

                    b.Property<int>("ShopId")
                        .HasColumnType("integer");

                    b.Property<DateTime>("Start")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("Stop")
                        .HasColumnType("timestamp with time zone");

                    b.Property<decimal>("SumAll")
                        .HasColumnType("numeric");

                    b.Property<decimal>("SumDiscount")
                        .HasColumnType("numeric");

                    b.Property<decimal>("SumElectron")
                        .HasColumnType("numeric");

                    b.Property<decimal>("SumIncome")
                        .HasColumnType("numeric");

                    b.Property<decimal>("SumNoElectron")
                        .HasColumnType("numeric");

                    b.Property<decimal>("SumOutcome")
                        .HasColumnType("numeric");

                    b.Property<decimal>("SumReturnElectron")
                        .HasColumnType("numeric");

                    b.Property<decimal>("SumReturnNoElectron")
                        .HasColumnType("numeric");

                    b.Property<decimal>("SumSell")
                        .HasColumnType("numeric");

                    b.Property<Guid>("Uuid")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("ShopId");

                    b.ToTable("Shifts");
                });

            modelBuilder.Entity("OnlineShop2.Database.Models.ShiftSummary", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<double>("Count")
                        .HasColumnType("double precision");

                    b.Property<decimal>("CountReturn")
                        .HasColumnType("numeric");

                    b.Property<int>("GoodId")
                        .HasColumnType("integer");

                    b.Property<int>("ShiftId")
                        .HasColumnType("integer");

                    b.Property<decimal>("Sum")
                        .HasColumnType("numeric");

                    b.Property<decimal>("SumReturn")
                        .HasColumnType("numeric");

                    b.HasKey("Id");

                    b.HasIndex("GoodId");

                    b.HasIndex("ShiftId");

                    b.ToTable("ShiftSummaries");
                });

            modelBuilder.Entity("OnlineShop2.Database.Models.Shop", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Adress")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Alias")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Inn")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Kpp")
                        .HasColumnType("text");

                    b.Property<int?>("LegacyDbNum")
                        .HasColumnType("integer");

                    b.Property<string>("OrgName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Shops");
                });

            modelBuilder.Entity("OnlineShop2.Database.Models.Supplier", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int?>("LegacyId")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("ShopId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("LegacyId");

                    b.HasIndex("ShopId");

                    b.ToTable("Suppliers");
                });

            modelBuilder.Entity("OnlineShop2.Database.Models.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<bool>("Active")
                        .HasColumnType("boolean");

                    b.Property<string>("Login")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Role")
                        .HasColumnType("integer");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("OnlineShop2.Database.Models.Barcode", b =>
                {
                    b.HasOne("OnlineShop2.Database.Models.Good", "Good")
                        .WithMany("Barcodes")
                        .HasForeignKey("GoodId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Good");
                });

            modelBuilder.Entity("OnlineShop2.Database.Models.CheckGood", b =>
                {
                    b.HasOne("OnlineShop2.Database.Models.CheckSell", "CheckSell")
                        .WithMany("CheckGoods")
                        .HasForeignKey("CheckSellId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("OnlineShop2.Database.Models.Good", "Good")
                        .WithMany("CheckGoods")
                        .HasForeignKey("GoodId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CheckSell");

                    b.Navigation("Good");
                });

            modelBuilder.Entity("OnlineShop2.Database.Models.CheckSell", b =>
                {
                    b.HasOne("OnlineShop2.Database.Models.Shift", "Shift")
                        .WithMany("CheckSells")
                        .HasForeignKey("ShiftId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Shift");
                });

            modelBuilder.Entity("OnlineShop2.Database.Models.Good", b =>
                {
                    b.HasOne("OnlineShop2.Database.Models.GoodGroup", "GoodGroup")
                        .WithMany("Goods")
                        .HasForeignKey("GoodGroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("OnlineShop2.Database.Models.Shop", "Shop")
                        .WithMany()
                        .HasForeignKey("ShopId");

                    b.HasOne("OnlineShop2.Database.Models.Supplier", "Supplier")
                        .WithMany("Goods")
                        .HasForeignKey("SupplierId");

                    b.Navigation("GoodGroup");

                    b.Navigation("Shop");

                    b.Navigation("Supplier");
                });

            modelBuilder.Entity("OnlineShop2.Database.Models.GoodCurrentBalance", b =>
                {
                    b.HasOne("OnlineShop2.Database.Models.Good", "Good")
                        .WithOne("CurrentBalance")
                        .HasForeignKey("OnlineShop2.Database.Models.GoodCurrentBalance", "GoodId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("OnlineShop2.Database.Models.Shop", "Shop")
                        .WithMany("GoodCurrentBalances")
                        .HasForeignKey("ShopId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Good");

                    b.Navigation("Shop");
                });

            modelBuilder.Entity("OnlineShop2.Database.Models.GoodGroup", b =>
                {
                    b.HasOne("OnlineShop2.Database.Models.Shop", "Shop")
                        .WithMany()
                        .HasForeignKey("ShopId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Shop");
                });

            modelBuilder.Entity("OnlineShop2.Database.Models.GoodPrice", b =>
                {
                    b.HasOne("OnlineShop2.Database.Models.Good", null)
                        .WithMany("GoodPrices")
                        .HasForeignKey("GoodId");

                    b.HasOne("OnlineShop2.Database.Models.Shop", "Shop")
                        .WithMany()
                        .HasForeignKey("ShopId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Shop");
                });

            modelBuilder.Entity("OnlineShop2.Database.Models.Inventory", b =>
                {
                    b.HasOne("OnlineShop2.Database.Models.Shop", "Shop")
                        .WithMany("Inventories")
                        .HasForeignKey("ShopId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Shop");
                });

            modelBuilder.Entity("OnlineShop2.Database.Models.InventoryGood", b =>
                {
                    b.HasOne("OnlineShop2.Database.Models.Good", "Good")
                        .WithMany("InventoryGoods")
                        .HasForeignKey("GoodId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("OnlineShop2.Database.Models.InventoryGroup", "InventoryGroup")
                        .WithMany("InventoryGoods")
                        .HasForeignKey("InventoryGroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Good");

                    b.Navigation("InventoryGroup");
                });

            modelBuilder.Entity("OnlineShop2.Database.Models.InventoryGroup", b =>
                {
                    b.HasOne("OnlineShop2.Database.Models.Inventory", "Inventory")
                        .WithMany("InventoryGroups")
                        .HasForeignKey("InventoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Inventory");
                });

            modelBuilder.Entity("OnlineShop2.Database.Models.InventorySummaryGood", b =>
                {
                    b.HasOne("OnlineShop2.Database.Models.Good", "Good")
                        .WithMany()
                        .HasForeignKey("GoodId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("OnlineShop2.Database.Models.InventoryGroup", null)
                        .WithMany("InventorySummaryGoods")
                        .HasForeignKey("InventoryGroupId");

                    b.HasOne("OnlineShop2.Database.Models.Inventory", "Inventory")
                        .WithMany("InventorySummaryGoods")
                        .HasForeignKey("InventoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Good");

                    b.Navigation("Inventory");
                });

            modelBuilder.Entity("OnlineShop2.Database.Models.RefreshToken", b =>
                {
                    b.HasOne("OnlineShop2.Database.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("OnlineShop2.Database.Models.Shift", b =>
                {
                    b.HasOne("OnlineShop2.Database.Models.Shop", "Shop")
                        .WithMany("Shifts")
                        .HasForeignKey("ShopId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Shop");
                });

            modelBuilder.Entity("OnlineShop2.Database.Models.ShiftSummary", b =>
                {
                    b.HasOne("OnlineShop2.Database.Models.Good", "Good")
                        .WithMany("ShiftSummaries")
                        .HasForeignKey("GoodId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("OnlineShop2.Database.Models.Shift", "Shift")
                        .WithMany("ShiftSummaries")
                        .HasForeignKey("ShiftId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Good");

                    b.Navigation("Shift");
                });

            modelBuilder.Entity("OnlineShop2.Database.Models.Supplier", b =>
                {
                    b.HasOne("OnlineShop2.Database.Models.Shop", "Shop")
                        .WithMany()
                        .HasForeignKey("ShopId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Shop");
                });

            modelBuilder.Entity("OnlineShop2.Database.Models.CheckSell", b =>
                {
                    b.Navigation("CheckGoods");
                });

            modelBuilder.Entity("OnlineShop2.Database.Models.Good", b =>
                {
                    b.Navigation("Barcodes");

                    b.Navigation("CheckGoods");

                    b.Navigation("CurrentBalance");

                    b.Navigation("GoodPrices");

                    b.Navigation("InventoryGoods");

                    b.Navigation("ShiftSummaries");
                });

            modelBuilder.Entity("OnlineShop2.Database.Models.GoodGroup", b =>
                {
                    b.Navigation("Goods");
                });

            modelBuilder.Entity("OnlineShop2.Database.Models.Inventory", b =>
                {
                    b.Navigation("InventoryGroups");

                    b.Navigation("InventorySummaryGoods");
                });

            modelBuilder.Entity("OnlineShop2.Database.Models.InventoryGroup", b =>
                {
                    b.Navigation("InventoryGoods");

                    b.Navigation("InventorySummaryGoods");
                });

            modelBuilder.Entity("OnlineShop2.Database.Models.Shift", b =>
                {
                    b.Navigation("CheckSells");

                    b.Navigation("ShiftSummaries");
                });

            modelBuilder.Entity("OnlineShop2.Database.Models.Shop", b =>
                {
                    b.Navigation("GoodCurrentBalances");

                    b.Navigation("Inventories");

                    b.Navigation("Shifts");
                });

            modelBuilder.Entity("OnlineShop2.Database.Models.Supplier", b =>
                {
                    b.Navigation("Goods");
                });
#pragma warning restore 612, 618
        }
    }
}
