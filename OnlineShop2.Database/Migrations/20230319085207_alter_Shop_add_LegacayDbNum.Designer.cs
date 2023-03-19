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
    [Migration("20230319085207_alter_Shop_add_LegacayDbNum")]
    partial class alter_Shop_add_LegacayDbNum
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

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("ShopId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

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

                    b.Property<double>("CountAppend")
                        .HasColumnType("double precision");

                    b.Property<double>("CountDB")
                        .HasColumnType("double precision");

                    b.Property<double>("CountFact")
                        .HasColumnType("double precision");

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

                    b.HasKey("Id");

                    b.HasIndex("InventoryId");

                    b.ToTable("InventoryGroups");
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

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("ShopId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

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

            modelBuilder.Entity("OnlineShop2.Database.Models.RefreshToken", b =>
                {
                    b.HasOne("OnlineShop2.Database.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
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

            modelBuilder.Entity("OnlineShop2.Database.Models.Good", b =>
                {
                    b.Navigation("Barcodes");

                    b.Navigation("CurrentBalance");

                    b.Navigation("GoodPrices");

                    b.Navigation("InventoryGoods");
                });

            modelBuilder.Entity("OnlineShop2.Database.Models.GoodGroup", b =>
                {
                    b.Navigation("Goods");
                });

            modelBuilder.Entity("OnlineShop2.Database.Models.Inventory", b =>
                {
                    b.Navigation("InventoryGroups");
                });

            modelBuilder.Entity("OnlineShop2.Database.Models.InventoryGroup", b =>
                {
                    b.Navigation("InventoryGoods");
                });

            modelBuilder.Entity("OnlineShop2.Database.Models.Shop", b =>
                {
                    b.Navigation("GoodCurrentBalances");

                    b.Navigation("Inventories");
                });

            modelBuilder.Entity("OnlineShop2.Database.Models.Supplier", b =>
                {
                    b.Navigation("Goods");
                });
#pragma warning restore 612, 618
        }
    }
}
