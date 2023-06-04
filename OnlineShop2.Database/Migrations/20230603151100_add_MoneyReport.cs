using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OnlineShop2.Database.Migrations
{
    /// <inheritdoc />
    public partial class add_MoneyReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MoneyReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ShopId = table.Column<int>(type: "integer", nullable: false),
                    Create = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StartGoodsSum = table.Column<decimal>(type: "numeric", nullable: false),
                    InventoryGoodsSum = table.Column<decimal>(type: "numeric", nullable: true),
                    InventoryCashMoney = table.Column<decimal>(type: "numeric", nullable: true),
                    ArrivalsSum = table.Column<decimal>(type: "numeric", nullable: true),
                    CashIncome = table.Column<decimal>(type: "numeric", nullable: true),
                    CashOutcome = table.Column<decimal>(type: "numeric", nullable: true),
                    CashMoney = table.Column<decimal>(type: "numeric", nullable: true),
                    CashElectron = table.Column<decimal>(type: "numeric", nullable: true),
                    Writeof = table.Column<decimal>(type: "numeric", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MoneyReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MoneyReports_Shops_ShopId",
                        column: x => x.ShopId,
                        principalTable: "Shops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MoneyReports_ShopId",
                table: "MoneyReports",
                column: "ShopId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MoneyReports");
        }
    }
}
