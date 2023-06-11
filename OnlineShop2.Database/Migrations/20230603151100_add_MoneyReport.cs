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
                    StopGoodSum = table.Column<decimal>(type: "numeric", nullable: false, defaultValue: 0m),
                    InventoryGoodsSum = table.Column<decimal>(type: "numeric", nullable: false, defaultValue: 0m),
                    InventoryCashMoney = table.Column<decimal>(type: "numeric", nullable: false, defaultValue: 0m),
                    ArrivalsSum = table.Column<decimal>(type: "numeric", nullable: false, defaultValue: 0m),
                    CashIncome = table.Column<decimal>(type: "numeric", nullable: false, defaultValue: 0m),
                    CashOutcome = table.Column<decimal>(type: "numeric", nullable: false, defaultValue: 0m),
                    CashMoney = table.Column<decimal>(type: "numeric", nullable: false, defaultValue: 0m),
                    CashElectron = table.Column<decimal>(type: "numeric", nullable: false, defaultValue: 0m),
                    RevaluationNew = table.Column<decimal>(type: "numeric", nullable: false, defaultValue: 0m),
                    RevaluationOld = table.Column<decimal>(type: "numeric", nullable: false, defaultValue: 0m),
                    MoneyItog = table.Column<decimal>(type: "numeric", nullable: false, defaultValue: 0m),
                    Writeof = table.Column<decimal>(type: "numeric", nullable: false, defaultValue: 0m)
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
